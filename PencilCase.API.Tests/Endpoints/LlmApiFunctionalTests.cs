using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using PencilCase.API.Handlers;
using PencilCase.API.Tests.Helpers;
using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.RAG;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Tests.Endpoints;

[TestFixture]
public class LlmApiFunctionalTests
{
    private ILlmApiEndpointsHandler _apiHandler;
    private Mock<IRagService> _ragServiceMock = null!;
    private Mock<ILlmApiService> _llmServiceMock = null!;
    
    private List<RagDocument> _capturedRagChunks = new();
    private List<RagDocument> _expectedRagChunks = new();
    private const string ExpectedAnswer = "Hmmm I'm almost sure that 1+1=3.";

    [SetUp]
    public void Setup()
    {
        SetupRagService();
        SetupLlmService();
        _apiHandler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmServiceMock.Object);
    }

    private void SetupRagService()
    {
        _ragServiceMock = new Mock<IRagService>();
        _ragServiceMock
            .Setup(service => service.GetChunksForQuery(
                It.IsAny<String>(), 
                It.IsAny<List<Guid>>(),
                It.IsAny<uint>()))
            .ReturnsAsync(_expectedRagChunks);

        _ragServiceMock
            .Setup<Task>(service => service.AddChunks(It.IsAny<List<RagDocument>>()))
            .Callback<List<RagDocument>>(
                chunkList => _capturedRagChunks.AddRange(chunkList))
            .Returns(Task.CompletedTask);
        
        _ragServiceMock
            .Setup(service => service.DeleteChunks(It.IsAny<List<RagDocument>>()))
            .Callback<List<RagDocument>>(
                docs => _capturedRagChunks.RemoveAll(docs.Contains));
    }

    private void SetupLlmService()
    {
        _llmServiceMock = new Mock<ILlmApiService>();
        _llmServiceMock.Setup(service => service.GenerateContent(
                It.IsAny<List<LlmMessage>>(),
                It.IsAny<LlmMessage?>()))
            .ReturnsAsync(new List<LlmMessage>()
            {
                new LlmMessage()
                {
                    Role = "model",
                    Content = ExpectedAnswer
                }
            });
    }

    [Test]
    public async Task UserGetsDocumentsFromRagAndUsesOnLlm()
    {
        // Carlos is studying mathematics and just started to use pencilcase, so he sets up his workspace
        var rootTopic = new Block()
        {
            Name = "Carlos's workspace",
        };

        var mathTopic = rootTopic.AddTopic("Math");
        var psychTopic = rootTopic.AddTopic("Psychology");
        
        // It's the end of the semester, so he is studying for multiple tests
        // so he adds some PDFs to his workspace, but not all PDFs are for math, since he also studies a psychology course
        
        var mathDocument1 = mathTopic
            .AddDocument("Math.pdf");
        var mathChunk1 = mathDocument1
            .AddChunk("Abstract: This paper explores interdisciplinary contexts where the expression \"1+1=3\" becomes valid through emergent phenomena and non-linear systems.", 0);
        var mathChunk2 = mathDocument1
            .AddChunk("The research combines foundations of mathematics, logic, philosophy, and social sciences...", 1);
        
        var algebraTopic = mathTopic.AddTopic("Arithmetic");
        var algebraChunk1 = algebraTopic
            .AddDocument("arithmetic.pdf")
            .AddChunk("This article does not aim to invalidate classical arithmetic...",0);
        
        var psychChunk = psychTopic
            .AddDocument("FrogPsychology.pdf")
            .AddChunk("Abstract: This study explores the behavioral and cognitive aspects of frogs...",0);
        
        var pdfs = new List<Block>()
        {
            mathChunk1,
            mathChunk2,
            algebraChunk1,
            psychChunk
        };
        
        // Under the hood pencilcase adds the information to its database
        var response = await _apiHandler.AddChunksAsync(pdfs);
        Assert.That(response, Is.InstanceOf<Ok>(), 
            "Handler returned IResult different than Ok when it should.");
        
        _expectedRagChunks.Add(new RagDocument(){ Id = mathChunk1.Id, ParentId = (Guid)mathChunk1.ParentId!, Content = mathChunk1.Name });
        _expectedRagChunks.Add(new RagDocument(){ Id = mathChunk2.Id, ParentId = (Guid)mathChunk2.ParentId!, Content = mathChunk2.Name });
        _expectedRagChunks.Add(new RagDocument(){ Id = algebraChunk1.Id, ParentId = (Guid)algebraChunk1.ParentId!, Content = algebraChunk1.Name });
        
        Assert.That(_capturedRagChunks, 
            Is.EquivalentTo(_expectedRagChunks),
            "Handler did not properly add chunks to RAG API.");
        
        // He, then, creates a new notebook and starts adding cells to it
        var firstQuestion = mathTopic
            .AddNotebook("really hard maths")
            .AddQuestion("What is 1+1?", 0);

        // he submits a cell he was working on, pencilcase starts looking for useful pieces of text
        response = await _apiHandler.SearchForChunksAsync(firstQuestion);
        Assert.That(response, Is.InstanceOf<Ok<List<Block>>>(), 
            "Handler did not return an OK response with a list of chunks.");

        var responseOk = response as Ok<List<Block>>;
        Assert.That(responseOk, Is.Not.Null, "Response cast to Ok was null.");
        Assert.That(responseOk.Value, Is.Not.Null.Or.Empty, 
            "Handler returned response with empty or null body.");
        
        var resultChunks = responseOk!.Value!.ToList();
        
        // After some time loading, pencilcase gets the relevant documents to the question and shows them to Carlos
        // He sees that the system returned the math pdfs he uploaded earlier
        var expectedChunks = new List<Block>()
        {
            mathChunk1,
            mathChunk2,
            algebraChunk1,
        };

        Assert.That(resultChunks, Is.Not.Empty, 
            "Handler returned an empty list.");
        
        Assert.That(
            resultChunks, 
            Is.EquivalentTo(expectedChunks),
            $"Chunks returned from Handler are different than expected.");
        
        // He sees that the sources it is using are not only from the same topic, but also from its subtopics
        // and not from the topics above
        Assert.That(resultChunks.Select(c => c.Parent).ToList(),
            Is.EquivalentTo(expectedChunks.Select(c => c.Parent).ToList()),
            "Expected parents for result chunks are different from actual result.");
        
        // Then, pencilcase sends the chunks to the LLM using the appropriate endpoint
        var chatMessages = new List<LlmMessage>
        {
            BuildLlmMessageFromQuestionAndDocs(firstQuestion, resultChunks)
        };

        var llmInvokeResponse =  await _apiHandler.InvokeAgentAsync(chatMessages);
        
        // It loads for a couple of seconds, but pencilcase finally shows him the answer to his question
        Assert.That(llmInvokeResponse, Is.InstanceOf<Ok<List<LlmMessage>>>(), 
            "Handler did not return an OK response with a list of chat messages.");
        
        var llmInvokeResponseOk = llmInvokeResponse as Ok<List<LlmMessage>>;
        Assert.That(llmInvokeResponseOk, Is.Not.Null, "Response cast to Ok was null.");
        Assert.That(llmInvokeResponseOk.Value, Is.Not.Null.Or.Empty, 
            "Handler returned response with empty or null body.");
        
        var resultAnswer = llmInvokeResponseOk!.Value!.ToList();
        Assert.That(resultAnswer, Is.Not.Null.And.Not.Empty, 
            "Answer list is null or empty.");
    }

    private List<LlmMessage> BuildLlmChatFromBlocks(Block blocks)
    {
        var result = new List<LlmMessage>();
        foreach (var block in blocks)
        {
            result.Add(new LlmMessage()
            {
                Role = "user",
                Content = block.Name
            });
            if (block.Children.Count > 0)
            {
                var answer = block.Children
                    .OrderByDescending(b => b.Properties!.Order)
                    .First();
                
                result.Add(new LlmMessage()
                {
                    Role = "model",
                    Content = answer.Name
                });
            }
        }
        return result;
    }

    private LlmMessage BuildLlmMessageFromQuestionAndDocs(Block question, List<Block> docs)
    {
        var docsText = string.Empty;
        for (int i = 0; i < docs.Count; i++)
        {
            var doc = docs[i];
            docsText += $"## CHUNK {i}\n{doc.Name}";
        }
        
        return new LlmMessage()
        {
            Role = "user",
            Content = docsText + $"\n## QUESTION\n{question.Name}"
        };
    }
}
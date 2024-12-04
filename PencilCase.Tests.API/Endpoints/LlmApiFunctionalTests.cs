using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using PencilCase.API.Handlers;
using PencilCase.Tests.API.Helpers;
using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.RAG;
using PencilCase.Shared.Data.Database;
using PencilCase.Shared.DTOs.Requests.Llm;
using PencilCase.Shared.DTOs.Requests.Rag;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Tests.API.Endpoints;

[TestFixture]
public class LlmApiFunctionalTests
{
    private ILlmApiEndpointsHandler _apiHandler;
    private Mock<IRagService> _ragServiceMock = null!;
    private Mock<ILlmApiService> _llmServiceMock = null!;
    private List<RagDocument> _capturedRagChunks = new();
    private Mock<IBlocksDal> _blocksDal;
    private const string ExpectedAnswer = "Hmmm I'm almost sure that 1+1=3.";

    [SetUp]
    public void Setup()
    {
        SetupMockRag();
        SetupMockLlm();
        SetupMockBlocksDal();
        _apiHandler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmServiceMock.Object, _blocksDal.Object);
    }

    private void SetupMockRag()
    {
        _ragServiceMock = new Mock<IRagService>();

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

    private void SetupMockLlm()
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
    
    private void SetupMockBlocksDal()
    {
        _blocksDal = new Mock<IBlocksDal>();
        _blocksDal.Setup(service => service.GetIdsFromSubtreeWithType(
                It.IsAny<Guid>(),
                It.IsAny<Func<Block, bool>>()))
            .Returns(new List<Guid>()
            {
                Guid.Empty
            });
    }

    [Test]
    public async Task UserGetsDocumentsFromRagAndUsesOnLlm()
    {
        // Carlos is studying mathematics and just started to use pencilcase, so he sets up his workspace
        var rootTopic = new Block()
        {
            Id = new Guid(),
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
        var addResponse = await _apiHandler.AddChunksAsync(pdfs.Select(b => new RagAddRequest()
        {
            Id = b.Id,
            ParentId = b.ParentId ?? Guid.Empty,
            Content = b.Name
        }));
        AssertAddingSuccesful(addResponse, pdfs);
        
        // He, then, creates a new notebook and starts adding cells to it
        var mathsNotebook = mathTopic
            .AddNotebook("really hard maths");
        var firstQuestion = mathsNotebook
            .AddQuestion("What is 1+1?", 0);

        // he submits a cell he was working on, pencilcase starts looking for useful pieces of text
        _blocksDal
            .Setup(service => service.GetBy(It.IsAny<Func<Block, bool>>()))
            .Returns(mathsNotebook);
        
        var expectedBlocks = new List<Block>()
        {
            mathChunk1,
            mathChunk2,
            algebraChunk1
        };
        
        _ragServiceMock.Setup(service =>
            service.GetChunksForQuery(It.IsAny<string>(), It.IsAny<List<Guid>>(), It.IsAny<uint>()))
            .ReturnsAsync(expectedBlocks.Select(b => new RagDocument()
            {
                Id = b.Id,
                ParentId = (Guid)b.ParentId!,
                Content = b.Name
            }).ToList());
        
        var searchResponse = await _apiHandler.SearchForChunksAsync(new RagSearchRequest()
        {
            NotebookId = firstQuestion.ParentId,
            Content = firstQuestion.Name,
        });
        
        // After some time loading, pencilcase gets the relevant documents to the question and shows them to Carlos
        // He sees that the system returned the math pdfs he uploaded earlier
        var resultChunks = AssertSearchResponseIsValidAndReturnContents(searchResponse);
        
        // He sees that the sources it is using are not only from the same topic, but also from its subtopics
        // and not from the topics above
        AssertSearchResponseContentIsValid(resultChunks, expectedBlocks);
        
        // Then, pencilcase sends the chunks to the LLM using the appropriate endpoint
        var chatMessages = new LlmMessageInvokeRequest()
        {
            ChatMessages = new List<LlmMessage>()
            {
                BuildLlmMessageFromQuestionAndDocs(firstQuestion, resultChunks)
            }
        };

        var llmInvokeResponse =  await _apiHandler.InvokeAgentAsync(chatMessages);
        
        // It loads for a couple of seconds, but pencilcase finally shows him the answer to his question
        AssertThatLlmResponseIsValid(llmInvokeResponse);
    }

    private List<LlmMessage> BuildLlmChatFromBlocks(List<Block> blocks)
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

    private LlmMessage BuildLlmMessageFromQuestionAndDocs(Block question, List<RagDocument> docs)
    {
        var docsText = string.Empty;
        for (int i = 0; i < docs.Count; i++)
        {
            var doc = docs[i];
            docsText += $"## CHUNK {i}\n{doc.Content}";
        }
        
        return new LlmMessage()
        {
            Role = "user",
            Content = docsText + $"\n## QUESTION\n{question.Name}"
        };
    }

    private void AssertAddingSuccesful(IResult response, List<Block> addedDocs)
    {
        Assert.That(response, Is.InstanceOf<Created>(), 
            "Handler returned IResult different than Created when it should.");
        
        Assert.That(_capturedRagChunks, 
            Is.EquivalentTo(addedDocs.Select(b => 
                new RagDocument(){ Id = b.Id, ParentId = (Guid)b.ParentId!, Content = b.Name })),
            "Handler did not properly add chunks to RAG API.");
    }
    
    private List<RagDocument> AssertSearchResponseIsValidAndReturnContents(IResult response)
    {
        Assert.That(response, Is.InstanceOf<Ok<List<RagDocument>>>(), 
            "Handler did not return an OK response with a list of RagDocuments when it should.");
        var responseOk = response as Ok<List<RagDocument>>;
        Assert.That(responseOk, Is.Not.Null, "Response cast to Ok was null.");

        return responseOk!.Value!.ToList();
    }
    
    private void AssertSearchResponseContentIsValid(List<RagDocument> responseContent, List<Block> expectedBlocks)
    {
        Assert.That(responseContent, Is.Not.Empty, 
            "Handler returned an empty list.");
        
        Assert.That(
            responseContent,
            Is.EquivalentTo(expectedBlocks.Select(b => new RagDocument()
            {
                Id = b.Id, 
                ParentId = (Guid)b.ParentId!, 
                Content = b.Name
            })),
            $"Chunks returned from Handler are different than expected.");
        
        Assert.That(responseContent.Select(c => c.ParentId).ToList(),
            Is.EquivalentTo(expectedBlocks.Select(c => c.ParentId).ToList()),
            "Expected ParentIds for expected blocks are different from actual RagDocuments returned by the endpoint.");
    }

    private void AssertThatLlmResponseIsValid(IResult response)
    {
        Assert.That(response, Is.InstanceOf<Ok<List<LlmMessage>>>(), 
            "Handler did not return an OK response with a list of chat messages.");
        
        var okResponse = response as Ok<List<LlmMessage>>;
        Assert.That(okResponse, Is.Not.Null, "Response cast to Ok was null.");
        Assert.That(okResponse.Value, Is.Not.Null.Or.Empty, 
            "Handler returned response with empty or null body.");
        
        var answer = okResponse!.Value!.ToList();
        Assert.That(answer, Is.Not.Null.And.Not.Empty, 
            "Answer list is null or empty.");
    }
}
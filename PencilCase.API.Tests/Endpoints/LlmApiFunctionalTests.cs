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
    private ILlmApiEndpointsHandler _apiHandler = null!;
    private Mock<IRagService> _ragServiceMock = null!;
    private Mock<ILlmApiService> _llmServiceMock = null!;
    
    private List<RagDocument> _ragDocsToReturn = new List<RagDocument>();

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
            .ReturnsAsync(_ragDocsToReturn);
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
                    Content = "Hmmm I'm almost sure that 1+1=3."
                }
            });
    }
    
    [Test]
    public void UserGetsDocumentsFromRagAndUsesOnLlm()
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
        var algebraChunk1 = algebraTopic.AddDocument("arithmetic.pdf")
            .AddChunk("This article does not aim to invalidate classical arithmetic...",0);
        
        var psychChunk = psychTopic
            .AddDocument("FrogPsychology.txt")
            .AddChunk("Abstract: This study explores the behavioral and cognitive aspects of frogs...",0);
        
        var pdfs = new List<Block>()
        {
            mathChunk1,
            mathChunk2,
            algebraChunk1,
            psychChunk
        };
        _apiHandler.AddChunksAsync(pdfs);
        
        // He, then, creates a new notebook and starts adding cells to it

        var firstQuestion = mathTopic
            .AddNotebook("really hard maths")
            .AddQuestion("What is 1+1?", 0);

        // he submits a cell he was working on
        // After some time loading, pencilcase gets the relevant documents to the question and shows them to Carlos
        _apiHandler.SearchForChunksAsync(firstQuestion);
        
        // He sees that the sources it is using are not only from the same topic, but also from its subtopics,
        // and not from its parent topics
        // Then, pencilcase sends them to the LLM using the appropriate endpoint
        // It loads for a couple of seconds, but pencilcase finally shows him the answer to his question
        // He sees the answer block as a child of the question block
        // He tries to change the question a little bit, since it wasn't helping much
        // Again, he waits for the answer to his question a little bit. pencilcase shows him the answer.
        // He then decides to delete this cell, since he felt the question wasn't really very useful to him
    }
}
using Moq;
using PencilCase.API.Handlers;
using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.RAG;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Tests.Endpoints;

[TestFixture]
public class NotebookApiFunctionalTests
{
    private ILlmApiEndpointsHandler _apiHandler = null!;
    private Mock<IRagService> _ragServiceMock = null!;
    private Mock<ILlmApiService> _llmServiceMock = null!;

    private static readonly Block RelatedBlock1 = new Block()
    {
        Name = "1+1=3",
    };
    
    private static readonly Block RelatedBlock2 = new Block()
    {
        Name = "Abstract: This paper explores interdisciplinary contexts where the expression \"1+1=3\" becomes valid through emergent phenomena and non-linear systems.",
    };
    
    private static readonly Block UnRelatedBlock1 = new Block()
    {
        Name = "Abstract: This study explores the behavioral and cognitive aspects of frogs, ...",
    };

    private static readonly Block QuestionBlock = new Block()
    {
        Id = Guid.Parse("661ea199-5931-4aaa-8c08-f2272c4eba70"),
        ParentId = Guid.Parse("cf36a4ce-3c22-4a63-9b67-ba224ca90f10"),
        Name = "What is 1+1?"
    };

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
            .ReturnsAsync(new List<RagDocument>()
            {
                new RagDocument()
                {
                    Content = RelatedBlock1.Name,
                    Id = RelatedBlock1.Id,
                    ParentId = (Guid)RelatedBlock1.ParentId!,
                },
                new RagDocument()
                {
                    Content = RelatedBlock2.Name,,
                    Id = RelatedBlock2.Id,
                    ParentId = (Guid)RelatedBlock2.ParentId!,
                }
            });
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
        // Carlos is studying mathematics and just started to use pencilcase,
        // It's the end of the semester, so he is studying for multiple tests
        // so he adds some PDFs to his workspace, but not all PDFs are for math, since he also studies a psychology course
        // He, then, creates a new notebook and starts adding cells to it
        // he submits a cell he was working on
        // After some time loading, pencilcase gets the relevant documents to the question and shows them to Carlos
        // Then, pencilcase sends them to the LLM using the appropriate endpoint
        // It loads for a couple of seconds, but pencilcase finally shows him the answer to his question
        // He tries to change the question a little bit, since it wasn't helping much
        // Again, he waits for the answer to his question a little bit. pencilcase shows him the answer.
        // He then decides to delete this cell, since he felt the question wasn't really very useful to him
        // 
    }
}
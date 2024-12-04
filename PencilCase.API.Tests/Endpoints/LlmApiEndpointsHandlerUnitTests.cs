using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using PencilCase.API.Handlers;
using PencilCase.API.Tests.Helpers;
using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.RAG;
using PencilCase.Shared.Data.Database;
using PencilCase.Shared.DTOs.Requests.Llm;
using PencilCase.Shared.DTOs.Requests.Rag;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Tests.Endpoints;

[TestFixture]
public class LlmApiEndpointsHandlerUnitTests
{
    private Mock<IRagService> _ragServiceMock = null!;
    private Mock<IBlocksDal> _blocksDalMock = null!;
    private Mock<ILlmApiService> _llmApiServiceMock = null!;
    
    [SetUp]
    public void SetUpTests()
    {
        _ragServiceMock = new();
        _blocksDalMock = new();
        _llmApiServiceMock = new();
    }
    
    [Test]
    public async Task AddChunksAsync_ShouldAddValidChunksBlocks()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);
        
        List<RagDocument> addedChunks = new();

        _ragServiceMock
            .Setup(s => s.AddChunks(It.IsAny<List<RagDocument>>()))
            .Callback<List<RagDocument>>(l => addedChunks.AddRange(l));

        var docsToAdd = new List<RagDocumentAddRequest>()
        {
            new() { Id = Guid.NewGuid(), Content = "test01", ParentId = Guid.NewGuid() },
        };
        
        await handler.AddChunksAsync(docsToAdd);
        
        Assert.That(addedChunks.Select(d => d.ToBlock()).ToList(), Is.EquivalentTo(docsToAdd));
    }
    
    [Test]
    public async Task AddChunksAsync_ShouldRespondWithBadRequest_IfDocsQuantityIsTooLarge()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);
        const uint maxDocsQuantity = 256;
        
        var docsToAdd = new List<RagDocumentAddRequest>() { };
        for (int i = 0; i < maxDocsQuantity + 1; i++)
        {
            docsToAdd.Add(new()
            {
                Id = Guid.NewGuid(), 
                Content = $"test{i}",
                ParentId = Guid.NewGuid()
            });
        }

        var response = await handler.AddChunksAsync(docsToAdd);
        
        Assert.That(response,
            Is.TypeOf<BadRequest>(),
            $"AddChunksAsync did not return BadRequest response with docs quantity of {maxDocsQuantity + 1}");
    }

    [Test]
    public async Task AddChunksAsync_ShouldRespondWithBadRequest_IfDocsQuantityIsZero()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);

        var docsToAdd = new List<RagDocumentAddRequest>() { };

        var response = await handler.AddChunksAsync(docsToAdd);

        Assert.That(response,
            Is.TypeOf<BadRequest>(),
            $"AddChunksAsync did not return BadRequest response with docs quantity of {docsToAdd.Count()}");
    }

    [Test]
    public async Task SearchForChunksAsync_ReturnsOkResponse_WithValidQueryBlock()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);
        
        var blockList = ExampleBlocksHelper.CreateRootWithSingleNotebook();
        var notebook = blockList.First(b => b.Type == BlockType.Notebook);
        var validQuestion = notebook.AddQuestion("What is 1+1?", 0);

        _blocksDalMock.Setup(s => s.GetBy(It.IsAny<Func<Block, bool>>()))
            .Returns(notebook);

        var response = await handler.SearchForChunksAsync(validQuestion.ToSearchRequest());
        
        Assert.That(response, Is.TypeOf<Ok>());
    }

    [Test]
    public async Task SearchForChunksAsync_ReturnsBadRequestResponse_IfQueryParentIdIsInvalid()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);

        var invalidQuestion = new RagDocumentSearchRequest()
            { NotebookId = Guid.NewGuid(), Content = "I dont have a parent \ud83e\udd79" };

        _blocksDalMock.Setup(s => s.GetBy(It.IsAny<Func<Block, bool>>()))
            .Returns(null as Block);

        var response = await handler.SearchForChunksAsync(invalidQuestion);
         
        Assert.That(response, Is.TypeOf<BadRequest<string>>());
    }
    
    [Test]
    public async Task SearchForChunksAsync_ReturnsBadRequestResponse_IfQueryHasNoGrandparent()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);

        var invalidNotebook = new Block()
        {
            Id = Guid.NewGuid(), ParentId = null, Name = "I dont have a parent \ud83e\udd79" 
        };
        var question = new Block()
        {
            Id = Guid.NewGuid(), ParentId = invalidNotebook.Id, Name = ":)"
        };

        _blocksDalMock.Setup(s => s.GetBy(It.IsAny<Func<Block, bool>>()))
            .Returns(invalidNotebook);

        var response = await handler.SearchForChunksAsync(question.ToSearchRequest());
        
        Assert.That(response, Is.TypeOf<BadRequest<string>>());
    }

    [Test]
    public async Task SearchForChunksAsync_UsesSubtreeIdsForFilters()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);

        var blockList = ExampleBlocksHelper.CreateRootWithSingleNotebook();
        var notebook = blockList.First(b => b.Type == BlockType.Notebook);
        var question = notebook.AddQuestion("What is 1+1?", 0);

        var expectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        
        _blocksDalMock.Setup(s => s.GetBy(It.IsAny<Func<Block, bool>>()))
            .Returns(notebook);
        _blocksDalMock.Setup(s => s.GetIdsFromSubtreeWithType(
                It.IsAny<Guid>(), 
                It.IsAny<Func<Block, bool>>()))
            .Returns(expectedIds);
        
        List<Guid> capturedIds = new();
        _ragServiceMock.Setup(s => s.GetChunksForQuery(
            It.IsAny<string>(),
            It.IsAny<List<Guid>>(),
            It.IsAny<uint>()))
            .Callback<string,List<Guid>,uint>((query, filters, nResults) => capturedIds = filters);

        await handler.SearchForChunksAsync(question.ToSearchRequest());
        
        Assert.That(capturedIds, Is.EquivalentTo(expectedIds));
    }

    [Test]
    public async Task SearchForChunksAsync_ReturnsBlocksWithCorrectIds()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);
        
        var expectedIds = new List<Guid>()
        {
            Guid.NewGuid(),
        };

        _blocksDalMock.Setup(s => s.GetBy(
                It.IsAny<Func<Block, bool>>()))
            .Returns(new Block()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.NewGuid(),
                Name = "test",
            });

        _blocksDalMock.Setup(s => s.GetIdsFromSubtreeWithType(
                It.IsAny<Guid>(),
                It.IsAny<Func<Block, bool>>()))
            .Returns(new List<Guid>());

        _ragServiceMock
            .Setup(s => s.GetChunksForQuery(It.IsAny <string>(), 
                It.IsAny<List<Guid>>(), 
                It.IsAny<uint>()))
            .ReturnsAsync(expectedIds
                .Select(id => new RagDocument(){ Id = id, ParentId = Guid.NewGuid(), Content = "" })
                .ToList());

        var result = await handler.SearchForChunksAsync(
            new RagDocumentSearchRequest()
            {
                NotebookId = Guid.NewGuid(), 
                Content = "test"
            });

        var okResponse = result as Ok<List<RagDocument>>;

        var responseContents = okResponse?.Value?.ToList();
        
        Assert.That(responseContents, Is.Not.Null);
        Assert.That(responseContents.Select(d => d.Id), Is.EquivalentTo(expectedIds));
    }

    [Test]
    public async Task SearchForChunksAsync_UsesQueryFilters()
    {
        throw new NotImplementedException();
    }
    
    [Test]
    public async Task SearchForChunksAsync_ReturnsAnswerWithCorrectContents()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);
        
        var expectedIds = new List<Guid>()
        {
            Guid.NewGuid(),
        };

        _blocksDalMock.Setup(s => s.GetBy(
                It.IsAny<Func<Block, bool>>()))
            .Returns(new Block()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.NewGuid(),
                Name = "test",
            });

        _blocksDalMock.Setup(s => s.GetIdsFromSubtreeWithType(
                It.IsAny<Guid>(),
                It.IsAny<Func<Block, bool>>()))
            .Returns(new List<Guid>());

        _ragServiceMock
            .Setup(s => s.GetChunksForQuery(It.IsAny <string>(), 
                It.IsAny<List<Guid>>(), 
                It.IsAny<uint>()))
            .ReturnsAsync(expectedIds
                .Select(id => new RagDocument(){ Id = id, ParentId = Guid.NewGuid(), Content = "" })
                .ToList());

        var result = await handler.SearchForChunksAsync(
            new RagDocumentSearchRequest()
            {
                NotebookId = Guid.NewGuid(), 
                Content = "test"
            });

        var okResponse = result as Ok<List<RagDocument>>;

        var responseContents = okResponse?.Value?.ToList();
        
        Assert.That(responseContents, Is.Not.Null);
        Assert.That(responseContents.Select(d => d.Id), Is.EquivalentTo(expectedIds));
    }
    
    [Test]
    public async Task SearchForChunksAsync_ReturnsEmptyCollection_IfChunksNotFound()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);
        
        var blockList = ExampleBlocksHelper.CreateRootWithSingleNotebook();
        var notebook = blockList.First(b => b.Type == BlockType.Notebook);
        var validQuestion = notebook.AddQuestion("What is 1+1?", 0);

        _blocksDalMock.Setup(s => s.GetBy(It.IsAny<Func<Block, bool>>()))
            .Returns(notebook);

        _ragServiceMock.Setup(s => s.GetChunksForQuery(
                It.IsAny<string>(),
                It.IsAny<List<Guid>>(),
                It.IsAny<uint>()))
            .ReturnsAsync(new List<RagDocument>());

        var response = await handler.SearchForChunksAsync(validQuestion.ToSearchRequest());
        var responseContent = ((Ok<List<RagDocument>>)response)!.Value?.ToList();
        
        Assert.That(responseContent, Is.Empty);
    }

    [Test]
    public async Task DeleteChunksAsync_DeletesValidChunks()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);

        var chunksDeleted = new List<RagDocument>();
        _ragServiceMock
            .Setup(s => s.DeleteChunks(It.IsAny<List<RagDocument>>()))
            .Callback<List<RagDocument>>(l => chunksDeleted.AddRange(l));

        var chunkToDelete = new RagDocumentDeleteRequest() { Id = Guid.NewGuid() };
        
        await handler.DeleteChunksAsync(new List<RagDocumentDeleteRequest>(){ chunkToDelete });
        Assert.That(chunksDeleted.Select(c => c.Id).ToList(),
            Is.EquivalentTo(new[] { chunkToDelete.Id }));
    }

    [Test]
    public async Task DeleteChunksAsync_ReturnsNoContentResponse_IfDeletedSuccessfully()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);

        var chunkToDelete = new RagDocumentDeleteRequest() { Id = Guid.NewGuid() };
        
        var response = await handler.DeleteChunksAsync(new List<RagDocumentDeleteRequest>(){ chunkToDelete });
        
        Assert.That(response, Is.TypeOf<NoContent>());
    }

    [Test]
    public async Task DeleteChunksAsync_ReturnsNotFound_IfChunkIdIsInvalid()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);
        
        _ragServiceMock
            .Setup(s => s.DeleteChunks(It.IsAny<List<RagDocument>>()))
            .Throws<ArgumentException>();
        
        var chunkToDelete = new RagDocumentDeleteRequest() { Id = Guid.NewGuid() };

        var response = await handler.DeleteChunksAsync(new List<RagDocumentDeleteRequest>(){ chunkToDelete });
        
        Assert.That(response, Is.TypeOf<NotFound>());
    }

    [Test]
    public async Task InvokeAgentAsync_ReturnsOkResponse_IfSuccessful()
    {
        var query = new LlmMessage()
        {
            Role = "user",
            Content = "This is a query from a hopefully human user"
        };
        
        var generatedContent = new LlmMessage()
        {
            Role = "model",
            Content = "This is a response from the wonderful LLM of your choice."
        };
        
        _llmApiServiceMock.Setup(s => s.GenerateContent(
                It.IsAny<List<LlmMessage>>(),
                It.IsAny<LlmMessage?>()))
            .ReturnsAsync(new List<LlmMessage>()
            {
                generatedContent
            });
        
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);

        var response = await handler.InvokeAgentAsync(new LlmMessageInvokeRequest()
        {
            ChatMessages = new List<LlmMessage>() { query }
        });
        
        Assert.That(response, Is.TypeOf<Ok<List<LlmMessage>>>());
    }

    [Test]
    public async Task InvokeAgentAsync_ReturnsCorrectLlmMessageContents()
    {
        var query = new LlmMessage()
        {
            Role = "user",
            Content = "This is a query from a hopefully human user"
        };
        
        var generatedContent = new LlmMessage()
        {
            Role = "model",
            Content = "This is a response from the wonderful LLM of your choice."
        };
        
        _llmApiServiceMock.Setup(s => s.GenerateContent(
            It.IsAny<List<LlmMessage>>(),
            It.IsAny<LlmMessage?>()))
            .ReturnsAsync(new List<LlmMessage>()
            {
                generatedContent
            });
        
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);

        var response = await handler.InvokeAgentAsync(new LlmMessageInvokeRequest()
        {
            ChatMessages = new List<LlmMessage>() { query }
        });
        var contents = ((Ok<List<LlmMessage>>)response!).Value?.ToList();
        
        Assert.That(contents!.First(), Is.EqualTo(generatedContent));
    }
    
    [Test]
    public async Task InvokeAgentAsync_ReturnsBadRequest_IfQueryContentIsEmpty()
    {
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);

        var response = await handler.InvokeAgentAsync(new LlmMessageInvokeRequest()
        {
            ChatMessages = new List<LlmMessage>() { }
        });
        
        Assert.That(response, Is.TypeOf<BadRequest>());
    }
    
    [Test]
    public async Task InvokeAgentAsync_Returns500_IfApiRaisesHttpRequestException()
    {
        var query = new LlmMessage()
        {
            Role = "user",
            Content = "This is a query from a hopefully human user"
        };
        
        _llmApiServiceMock.Setup(s => s.GenerateContent(
                It.IsAny<List<LlmMessage>>(),
                It.IsAny<LlmMessage?>()))
            .Throws<HttpRequestException>();
        
        var handler = new LlmApiEndpointsHandler(_ragServiceMock.Object, _llmApiServiceMock.Object, _blocksDalMock.Object);

        var response = await handler.InvokeAgentAsync(new LlmMessageInvokeRequest()
        {
            ChatMessages = new List<LlmMessage>() { query }
        });
        
        Assert.That(response, Is.EqualTo(Results.StatusCode(500)));
    }
}
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using PencilCase.API.Handlers;
using PencilCase.API.Tests.Helpers;
using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.RAG;
using PencilCase.Shared.Data.Database;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Tests.Endpoints;

[TestFixture]
public class LlmApiEndpointsHandlerUnitTests
{
    [Test]
    public async Task AddChunksAsync_ShouldAddValidChunksBlocks()
    {
        Mock<IRagService> ragServiceMock = new();
        Mock<IBlocksDal> blocksDalMock = new();
        Mock<ILlmApiService> llmApiServiceMock = new();
        
        List<RagDocument> addedChunks = new();

        ragServiceMock
            .Setup(s => s.AddChunks(It.IsAny <List<RagDocument>>()))
            .Callback<List<RagDocument>>(l => addedChunks.AddRange(l));

        var handler = new LlmApiEndpointsHandler(ragServiceMock.Object, llmApiServiceMock.Object, blocksDalMock.Object);

        var docsToAdd = new List<Block>()
        {
            new() { Id = Guid.NewGuid(), Name = "test01", Type = BlockType.Cell, ParentId = Guid.NewGuid() },
        };
        
        await handler.AddChunksAsync(docsToAdd);
        
        Assert.That(addedChunks.Select(d => d.ToBlock()).ToList(), Is.EquivalentTo(docsToAdd));
    }
    
    [Test]
    public async Task AddChunksAsync_ShouldRespondWithBadRequest_IfDocsQuantityIsTooLarge()
    {
        Mock<IRagService> ragServiceMock = new();
        Mock<IBlocksDal> blocksDalMock = new();
        Mock<ILlmApiService> llmApiServiceMock = new();
        const uint maxDocsQuantity = 256;

        var handler = new LlmApiEndpointsHandler(ragServiceMock.Object, llmApiServiceMock.Object, blocksDalMock.Object);
        
        var docsToAdd = new List<Block>() { };
        for (int i = 0; i < maxDocsQuantity + 1; i++)
        {
            docsToAdd.Add(new()
            {
                Id = Guid.NewGuid(), 
                Name = $"test{i}", 
                Type = BlockType.Cell, 
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
        Mock<IRagService> ragServiceMock = new();
        Mock<IBlocksDal> blocksDalMock = new();
        Mock<ILlmApiService> llmApiServiceMock = new();

        var handler = new LlmApiEndpointsHandler(ragServiceMock.Object, llmApiServiceMock.Object, blocksDalMock.Object);

        var docsToAdd = new List<Block>() { };

        var response = await handler.AddChunksAsync(docsToAdd);

        Assert.That(response,
            Is.TypeOf<BadRequest>(),
            $"AddChunksAsync did not return BadRequest response with docs quantity of {docsToAdd.Count()}");
    }

    [Test]
    public async Task SearchForChunksAsync_ReturnsOkResponse_WithValidQueryBlock()
    {
        Mock<IRagService> ragServiceMock = new();
        Mock<ILlmApiService> llmApiServiceMock = new();
        Mock<IBlocksDal> blocksDalMock = new();
        var handler = new LlmApiEndpointsHandler(ragServiceMock.Object, llmApiServiceMock.Object, blocksDalMock.Object);

        var blockList = ExampleBlocksHelper.CreateRootWithSingleNotebook();
        var notebook = blockList.First(b => b.Type == BlockType.Notebook);
        var validQuestion = notebook.AddQuestion("What is 1+1?", 0);

        blocksDalMock.Setup(s => s.GetBy(It.IsAny<Func<Block, bool>>()))
            .Returns(notebook);

        var response = await handler.SearchForChunksAsync(validQuestion);
        
        Assert.That(response, Is.TypeOf<Ok>());
    }

    [Test]
    public async Task SearchForChunksAsync_ReturnsBadRequestResponse_IfQueryParentIdIsInvalid()
    {
        Mock<IRagService> ragServiceMock = new();
        Mock<ILlmApiService> llmApiServiceMock = new();
        Mock<IBlocksDal> blocksDalMock = new();
        var handler = new LlmApiEndpointsHandler(ragServiceMock.Object, llmApiServiceMock.Object, blocksDalMock.Object);

        var invalidQuestion = new Block()
            { Id = Guid.NewGuid(), ParentId = Guid.NewGuid(), Name = "I dont have a parent \ud83e\udd79" };

        blocksDalMock.Setup(s => s.GetBy(It.IsAny<Func<Block, bool>>()))
            .Returns(null as Block);

        var response = await handler.SearchForChunksAsync(invalidQuestion);
        
        Assert.That(response, Is.TypeOf<BadRequest<string>>());
    }
    
    [Test]
    public async Task SearchForChunksAsync_ReturnsBadRequestResponse_IfQueryHasNoGrandparent()
    {
        Mock<IRagService> ragServiceMock = new();
        Mock<ILlmApiService> llmApiServiceMock = new();
        Mock<IBlocksDal> blocksDalMock = new();
        var handler = new LlmApiEndpointsHandler(ragServiceMock.Object, llmApiServiceMock.Object, blocksDalMock.Object);

        var invalidNotebook = new Block()
        {
            Id = Guid.NewGuid(), ParentId = null, Name = "I dont have a parent \ud83e\udd79" 
        };
        var question = new Block()
        {
            Id = Guid.NewGuid(), ParentId = invalidNotebook.Id, Name = ":)"
        };

        blocksDalMock.Setup(s => s.GetBy(It.IsAny<Func<Block, bool>>()))
            .Returns(invalidNotebook);

        var response = await handler.SearchForChunksAsync(question);
        
        Assert.That(response, Is.TypeOf<BadRequest<string>>());
    }

    [Test]
    public async Task SearchForChunksAsync_UsesSubtreeIdsForFilters()
    {
        Mock<IRagService> ragServiceMock = new();
        Mock<ILlmApiService> llmApiServiceMock = new();
        Mock<IBlocksDal> blocksDalMock = new();
        var handler = new LlmApiEndpointsHandler(ragServiceMock.Object, llmApiServiceMock.Object, blocksDalMock.Object);

        var blockList = ExampleBlocksHelper.CreateRootWithSingleNotebook();
        var notebook = blockList.First(b => b.Type == BlockType.Notebook);
        var question = notebook.AddQuestion("What is 1+1?", 0);

        var expectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        
        blocksDalMock.Setup(s => s.GetBy(It.IsAny<Func<Block, bool>>()))
            .Returns(notebook);
        blocksDalMock.Setup(s => s.GetIdsFromSubtreeWithType(
                It.IsAny<Guid>(), 
                It.IsAny<Func<Block, bool>>()))
            .Returns(expectedIds);
        
        List<Guid> capturedIds = new();
        ragServiceMock.Setup(s => s.GetChunksForQuery(
            It.IsAny<string>(),
            It.IsAny<List<Guid>>(),
            It.IsAny<uint>()))
            .Callback<string,List<Guid>,uint>((query, filters, nResults) => capturedIds = filters);

        var response = await handler.SearchForChunksAsync(question);
        
        Assert.That(capturedIds, Is.EquivalentTo(expectedIds));
    }

    [Test]
    public async Task SearchForChunksAsync_ReturnsBlocksWithCorrectIds()
    {
        Mock<IRagService> ragServiceMock = new();
        Mock<IBlocksDal> blocksDalMock = new();
        Mock<ILlmApiService> llmApiServiceMock = new();
        
        List<RagDocument> addedChunks = new();
        
        var expectedIds = new List<Guid>()
        {
            Guid.NewGuid(),
        };

        blocksDalMock.Setup(s => s.GetBy(
                It.IsAny<Func<Block, bool>>()))
            .Returns(new Block()
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.NewGuid(),
                Name = "test",
            });

        blocksDalMock.Setup(s => s.GetIdsFromSubtreeWithType(
                It.IsAny<Guid>(),
                It.IsAny<Func<Block, bool>>()))
            .Returns(new List<Guid>());

        ragServiceMock
            .Setup(s => s.GetChunksForQuery(It.IsAny <string>(), 
                It.IsAny<List<Guid>>(), 
                It.IsAny<uint>()))
            .ReturnsAsync(expectedIds
                .Select(id => new RagDocument(){ Id = id, ParentId = Guid.NewGuid(), Content = "" })
                .ToList());

        var handler = new LlmApiEndpointsHandler(ragServiceMock.Object, llmApiServiceMock.Object, blocksDalMock.Object);
        var result = await handler.SearchForChunksAsync(
            new Block()
            {
                Id = Guid.NewGuid(), 
                ParentId = Guid.NewGuid(), 
                Name = "test"
            });

        var okResponse = result as Ok<List<RagDocument>>;

        var responseContents = okResponse?.Value?.ToList();
        
        Assert.That(responseContents, Is.Not.Null);
        Assert.That(responseContents.Select(d => d.Id), Is.EquivalentTo(expectedIds));
    }
    
    [Test]
    public async Task SearchForChunksAsync_ReturnsBlocksWithCorrectContents()
    {
        throw new NotImplementedException();
    }
    
    [Test]
    public async Task SearchForChunksAsync_ReturnsEmptyCollection_IfChunksNotFound()
    {
        throw new NotImplementedException();
    }

    [Test]
    public async Task DeleteChunksAsync_DeletesValidChunks()
    {
        throw new NotImplementedException();
    }

    [Test]
    public async Task DeleteChunksAsync_RaisesArgumentException_IfChunkIdIsInvalid()
    {
        throw new NotImplementedException();
    }

    [Test]
    public async Task InvokeAgentAsync_ReturnsCorrectLlmMessageContents()
    {
        throw new NotImplementedException();
    }
    
    [Test]
    public async Task InvokeAgentAsync_RaisesArgumentException_IfQueryContentIsEmpty()
    {
        throw new NotImplementedException();
    }
    
    [Test]
    public async Task InvokeAgentAsync_RaisesHttpRequestException_IfApiRaisesHttpRequestException()
    {
        throw new NotImplementedException();
    }
}
using Microsoft.AspNetCore.Http;
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
    public async Task AddChunksAsync_ShouldRaiseException_IfApiRaisesInvalidOperationException()
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

    {
        
    }

    [Test]
    public async Task SearchForChunksAsync_ReturnsBlocksWithCorrectIds()
    {
        
    }
    
    [Test]
    public async Task SearchForChunksAsync_ReturnsBlocksWithCorrectContents()
    {
        
    }
    
    [Test]
    public async Task SearchForChunksAsync_ReturnsEmptyCollection_IfChunksNotFound()
    {
        
    }

    [Test]
    public async Task SearchForChunksAsync_RaisesArgumentException_IfQueryParentIdIsInvalid()
    {
        
    }

    [Test]
    public async Task DeleteChunksAsync_DeletesValidChunks()
    {
        
    }

    [Test]
    public async Task DeleteChunksAsync_RaisesArgumentException_IfChunkIdIsInvalid()
    {
        
    }

    [Test]
    public async Task InvokeAgentAsync_ReturnsCorrectLlmMessageContents()
    {
        
    }
    
    [Test]
    public async Task InvokeAgentAsync_RaisesArgumentException_IfQueryContentIsEmpty()
    {
        
    }
    
    [Test]
    public async Task InvokeAgentAsync_RaisesHttpRequestException_IfApiRaisesHttpRequestException()
    {
        
    }
}
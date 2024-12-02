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
    public async Task AddChunksAsync_ShouldRaiseException_IfChunksAlreadyAdded()
    {
        
    }
    
    [Test]
    public async Task AddChunksAsync_ShouldRaiseException_IfDocsQuantityIsTooLarge()
    {
        
    }
    
    [Test]
    public async Task AddChunksAsync_ShouldRaiseException_IfDocsQuantityIsZero()
    {
        
    }
    
    [Test]
    public async Task AddChunksAsync_ShouldRaiseException_IfChunkIsWrongType()
    {
        
    }
    
    [Test]
    public async Task AddChunksAsync_ShouldRaiseException_IfApiRaisesInvalidOperationException()
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
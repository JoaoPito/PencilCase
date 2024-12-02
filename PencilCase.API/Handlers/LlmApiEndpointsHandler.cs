using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.RAG;
using PencilCase.Shared.Data.Database;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Handlers;

public class LlmApiEndpointsHandler : ILlmApiEndpointsHandler
{
    private readonly IRagService _ragService;
    private readonly ILlmApiService _llmApiService;
    private readonly BlocksDAL _blocksDal;

    public LlmApiEndpointsHandler(
        IRagService ragService, 
        ILlmApiService llmApiService, 
        BlocksDAL blocksDal)
    {
        _ragService = ragService;
        _llmApiService = llmApiService;
        _blocksDal = blocksDal;
    }

    public async Task<IResult> AddChunksAsync(IEnumerable<Block> chunks)
    {
        var chunkList = chunks.ToList();
        if(chunkList.Count is < 1 or > 250)
            return Results.BadRequest();

        var docsList = MapBlockListToRagDocumentsList(chunkList);
                
        await _ragService.AddChunks(docsList);
        return Results.Created();
    }

    private List<RagDocument> MapBlockListToRagDocumentsList(List<Block> blockList)
    {
        return blockList.Select(b => new RagDocument()
        {
            Id = b.Id,
            ParentId = b.ParentId ?? Guid.Empty,
            Content = b.Name,
        }).ToList();
    }

    public async Task<IResult> SearchForChunksAsync(Block query)
    {
        if(query.Name == string.Empty)
            return Results.BadRequest();

        var notebookBlock = _blocksDal.GetBy(b => b.Id == query.ParentId)
                            ?? throw new ArgumentException("Query block does not have a valid parent!");
        if(notebookBlock.ParentId is null)
            throw new ArgumentException("Query block does not have a valid grandparent!");
        
        var filterIds = _blocksDal.GetIdsFromSubtreeWithType((Guid)notebookBlock.ParentId!, b => b.Type == BlockType.Source);
                
        var docs = await _ragService.GetChunksForQuery(query.Name, filterIds, 3);
        return Results.Ok(docs);
    }

    public async Task<IResult> InvokeAgentAsync(List<LlmMessage> chat)
    {
        if(chat.Count < 1)
            return Results.BadRequest();
        return Results.Ok(await _llmApiService.GenerateContent(chat));
    }

    public async Task<IResult> DeleteChunksAsync(List<Block> chunks)
    {
        try
        {
            await _ragService.DeleteChunks(chunks.Select(b =>
                new RagDocument()
                {
                    Id = b.Id,
                    ParentId = b.ParentId ?? Guid.Empty,
                    Content = b.Name,
                }).ToList());
        }
        catch (ArgumentException)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    }
}
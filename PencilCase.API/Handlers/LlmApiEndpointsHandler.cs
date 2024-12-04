using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.RAG;
using PencilCase.Shared.Data.Database;
using PencilCase.Shared.DTOs.Requests.Llm;
using PencilCase.Shared.DTOs.Requests.Rag;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Handlers;

public class LlmApiEndpointsHandler : ILlmApiEndpointsHandler
{
    private readonly IRagService _ragService;
    private readonly ILlmApiService _llmApiService;
    private readonly IBlocksDal _blocksDal;

    public LlmApiEndpointsHandler(
        IRagService ragService, 
        ILlmApiService llmApiService, 
        IBlocksDal blocksDal)
    {
        _ragService = ragService;
        _llmApiService = llmApiService;
        _blocksDal = blocksDal;
    }

    public async Task<IResult> AddChunksAsync(IEnumerable<RagAddRequest> chunks)
    {
        var chunkList = chunks.ToList();
        if(chunkList.Count is < 1 or > 256)
            return Results.BadRequest();

        var docsList = MapRequestsListToRagDocumentsList(chunkList);
                
        await _ragService.AddChunks(docsList);
        return Results.Created();
    }

    private List<RagDocument> MapRequestsListToRagDocumentsList(List<RagAddRequest> requestsList)
    {
        return requestsList.Select(r => new RagDocument()
        {
            Id = r.Id,
            ParentId = r.ParentId,
            Content = r.Content,
        }).ToList();
    }

    public async Task<IResult> SearchForChunksAsync(RagDocumentSearchRequest query)
    {
        if(query.Content == string.Empty || 
           (query.NotebookId is null && query.FilterIds is null))
            return Results.BadRequest();

        var notebookBlock = _blocksDal.GetBy(b => b.Id == query.NotebookId);
        
        try
        {
            ValidateNotebookBlockForSearch(notebookBlock);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        
        var filterIds = _blocksDal.GetIdsFromSubtreeWithType(
            (Guid)notebookBlock!.ParentId!,
            b => b.Type == BlockType.Source);
                
        var docs = await _ragService.GetChunksForQuery(query.Content, filterIds, 3);
        return Results.Ok(docs);
    }

    public async Task<IResult> InvokeAgentAsync(LlmMessageInvokeRequest request)
    {
        if(request.ChatMessages.Count < 1)
            return Results.BadRequest();
        try
        {
            return Results.Ok(await _llmApiService.GenerateContent(request.ChatMessages));
        }
        catch (HttpRequestException)
        {
            return Results.StatusCode(500);
        }
    }

    public async Task<IResult> DeleteChunksAsync(List<RagDocumentDeleteRequest> chunks)
    {
        try
        {
            await _ragService.DeleteChunks(chunks.Select(b =>
                new RagDocument()
                {
                    Id = b.Id,
                    ParentId = b.ParentId ?? Guid.Empty,
                    Content = b.Content ?? String.Empty,
                }).ToList());
        }
        catch (ArgumentException)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    }

    private void ValidateNotebookBlockForSearch(Block? notebookBlock)
    {
        if(notebookBlock is null) 
            throw new ArgumentException("Query block does not have a valid parent!");
        
        if(notebookBlock.ParentId is null)
            throw new ArgumentException("Query block does not have a valid grandparent!");
    }
}
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
    
    public async Task<IResult> SearchForChunksAsync(RagSearchRequest query)
    {
        if(query.Content == string.Empty || 
           (query.NotebookId is null && query.FilterIds is null))
            return Results.BadRequest();

        var filterIds = new List<Guid>();

        if (query.FilterIds is not null)
        {
            filterIds = query.FilterIds.ToList();
        }
        else
        {
            try
            {
                var topicId = GetTopicIdFromId((Guid)query.NotebookId!);
                
                filterIds = _blocksDal.GetIdsFromSubtreeWithType(
                    topicId,
                    b => b.Type == BlockType.Source);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }
        
        var docs = await _ragService.GetChunksForQuery(query.Content, filterIds, 3);
        return Results.Ok(docs);
    }

    private Guid GetTopicIdFromId(Guid id)
    {
        var notebookBlock = _blocksDal.GetBy(b => b.Id == id);
        ValidateNotebookBlockForSearch(notebookBlock);
        return (Guid)notebookBlock!.ParentId!;
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

    public async Task<IResult> DeleteChunksAsync(RagDeleteRequest request)
    {
        try
        {
            await _ragService.DeleteChunks(request.ChunksIds.Select(b =>
                new RagDocument()
                {
                    Id = b,
                    ParentId = request.DocumentId
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
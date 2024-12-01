using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.RAG;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Handlers;

public class LlmApiEndpointsHandler : ILlmApiEndpointsHandler
{
    private readonly IRagService _ragService;
    private readonly ILlmApiService _llmApiService;

    public LlmApiEndpointsHandler(IRagService ragService, ILlmApiService llmApiService)
    {
        _ragService = ragService;
        _llmApiService = llmApiService;
    }

    public Task<IResult> AddChunksAsync(IEnumerable<Block> chunks)
    {
        throw new NotImplementedException();
    }

    public Task<IResult> SearchForChunksAsync(Block query)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> InvokeAgentAsync(List<LlmMessage> chat)
    {
        if(chat.Count < 1)
            return Results.BadRequest();
        return Results.Ok(await _llmApiService.GenerateContent(chat));
    }

    public Task<IResult> DeleteChunksAsync(List<Block> chunks)
    {
        throw new NotImplementedException();
    }
}
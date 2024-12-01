using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.RAG;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Handlers;

public class LlmApiEndpointsHandler : ILlmApiEndpointsHandler
{
    public LlmApiEndpointsHandler(IRagService ragService, ILlmApiService llmApiService)
    {
        
    }

    public Task AddChunksAsync(IEnumerable<Block> chunks)
    {
        throw new NotImplementedException();
    }

    public Task<List<Block>> SearchForChunksAsync(Block query)
    {
        throw new NotImplementedException();
    }

    public Task<List<LlmMessage>> InvokeAgentAsync(List<LlmMessage> chat, List<Block>? docs)
    {
        throw new NotImplementedException();
    }

    public Task DeleteChunksAsync(List<Block> chunks)
    {
        throw new NotImplementedException();
    }
}
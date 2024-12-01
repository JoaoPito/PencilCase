using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Handlers;

public interface ILlmApiEndpointsHandler
{
    public Task AddChunksAsync(IEnumerable<Block> chunks);
    public Task<List<Block>> SearchForChunksAsync(Block query);
    public Task<List<LlmMessage>> InvokeAgentAsync(List<LlmMessage> chat, List<Block>? docs);
    public Task DeleteChunksAsync(List<Block> chunks);
}
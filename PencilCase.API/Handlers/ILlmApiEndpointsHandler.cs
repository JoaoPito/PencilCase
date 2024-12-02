using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Handlers;

public interface ILlmApiEndpointsHandler
{
    public Task<IResult> AddChunksAsync(IEnumerable<Block> chunks);
    public IResult SearchForChunks(Block query);
    public Task<IResult> InvokeAgentAsync(List<LlmMessage> chat);
    public Task<IResult> DeleteChunksAsync(List<Block> chunks);
}
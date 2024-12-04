using PencilCase.Shared.DTOs.Requests.Rag;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Handlers;

public interface ILlmApiEndpointsHandler
{
    public Task<IResult> AddChunksAsync(IEnumerable<RagDocumentAddRequest> chunks);
    public Task<IResult> SearchForChunksAsync(RagDocumentSearchRequest query);
    public Task<IResult> InvokeAgentAsync(List<LlmMessage> chat);
    public Task<IResult> DeleteChunksAsync(List<Block> chunks);
}
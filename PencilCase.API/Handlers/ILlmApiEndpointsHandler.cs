using PencilCase.Shared.DTOs.Requests.Llm;
using PencilCase.Shared.DTOs.Requests.Rag;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Handlers;

public interface ILlmApiEndpointsHandler
{
    public Task<IResult> AddChunksAsync(IEnumerable<RagAddRequest> chunks);
    public Task<IResult> SearchForChunksAsync(RagSearchRequest query);
    public Task<IResult> InvokeAgentAsync(LlmMessageInvokeRequest request);
    public Task<IResult> DeleteChunksAsync(RagDeleteRequest request);
}
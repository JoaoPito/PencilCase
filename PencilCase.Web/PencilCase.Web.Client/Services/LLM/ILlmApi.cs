using PencilCase.Shared.DTOs.Requests.Llm;
using PencilCase.Shared.DTOs.Requests.Rag;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;

namespace PencilCase.Web.Client.Services.LLM;

public interface ILlmApi
{
    public Task<IEnumerable<LlmMessage>> InvokeLlmAgentAsync(LlmMessageInvokeRequest chatRequest);
    public Task AddRagDocumentsAsync(IEnumerable<RagAddRequest> documents);
    public Task<IEnumerable<RagDocument>> QueryRagDocumentsAsync(RagSearchRequest request);
    public Task DeleteRagDocumentsAsync(RagDeleteRequest request);
}
using PencilCase.Shared.DTOs.Requests.Llm;
using PencilCase.Shared.DTOs.Requests.Rag;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;

namespace PencilCase.Web.Services.LLM;

public interface ILlmApi
{
    public Task<IEnumerable<LlmMessage>> InvokeLlmAgentAsync(IEnumerable<LlmMessageInvokeRequest> llmChat);
    public Task AddRagDocumentsAsync(IEnumerable<RagAddRequest> documents);
    public Task<IEnumerable<RagDocument>> QueryRagDocumentsAsync(RagSearchRequest request);
    public Task DeleteRagDocumentsAsync(IEnumerable<RagDeleteRequest> documents);
}
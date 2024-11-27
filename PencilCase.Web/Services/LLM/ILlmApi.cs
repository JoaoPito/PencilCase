using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;

namespace PencilCase.Web.Services.LLM;

public interface ILlmApi
{
    public Task<LlmMessage> InvokeLlmAgentAsync(IEnumerable<LlmMessage> llmChat);
    public Task AddRagDocumentsAsync(IEnumerable<RagDocument> documents);
    public Task<IEnumerable<RagDocument>> QueryRagDocumentsAsync(string llmId);
    public Task DeleteRagDocumentsAsync(IEnumerable<RagDocument> documents);
}
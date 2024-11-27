using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;

namespace PencilCase.Web.Services.LLM;

public interface ILlmApi
{
    public Task<IEnumerable<LlmMessage>> InvokeLlmAgentAsync(IEnumerable<LlmMessage> llmChat);
    public Task AddRagDocumentsAsync(IEnumerable<RagDocument> documents);
    public Task<IEnumerable<RagDocument>> QueryRagDocumentsAsync(string query, List<Guid> parentIds, uint? nResults=3);
    public Task DeleteRagDocumentsAsync(IEnumerable<RagDocument> documents);
}
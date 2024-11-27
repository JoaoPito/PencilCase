using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;

namespace PencilCase.Web.Services.LLM;

public class LlmApi : ILlmApi
{
    private readonly HttpClient _httpClient;

    public LlmApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("LlmApi");
    }
    
    public Task<LlmMessage> InvokeLlmAgentAsync(IEnumerable<LlmMessage> llmChat)
    {
        throw new NotImplementedException();
    }

    public Task AddRagDocumentsAsync(IEnumerable<RagDocument> documents)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<RagDocument>> QueryRagDocumentsAsync(string llmId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteRagDocumentsAsync(IEnumerable<RagDocument> documents)
    {
        throw new NotImplementedException();
    }
}
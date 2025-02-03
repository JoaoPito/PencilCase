using PencilCase.Shared.DTOs.Requests.Llm;
using PencilCase.Shared.DTOs.Requests.Rag;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;

namespace PencilCase.Web.Client.Services.LLM;

public class LlmApi : ILlmApi
{
    private readonly HttpClient _httpClient;

    public LlmApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("LlmAPI");
    }
    
    public async Task<IEnumerable<LlmMessage>>  InvokeLlmAgentAsync(LlmMessageInvokeRequest chatRequest)
    {
        var response = await _httpClient.PostAsJsonAsync("agent/invoke", chatRequest);
        response.EnsureSuccessStatusCode();
        var responseContents = await response.Content.ReadFromJsonAsync<List<LlmMessage>>() ?? new List<LlmMessage>();
        return responseContents;
    }

    public async Task AddRagDocumentsAsync(IEnumerable<RagAddRequest> documents)
    {
        var response = await _httpClient.PostAsJsonAsync("rag", documents);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<RagDocument>> QueryRagDocumentsAsync(RagSearchRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("rag/search", request);
        
        response.EnsureSuccessStatusCode();
        var responseContents = await response.Content.ReadFromJsonAsync<List<RagDocument>>()
                               ?? new List<RagDocument>();
        return responseContents;
    }

    public async Task DeleteRagDocumentsAsync(RagDeleteRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("rag/delete", request);
        response.EnsureSuccessStatusCode();
    }
}
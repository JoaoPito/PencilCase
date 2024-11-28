using System.Net.Http.Json;
using PencilCase.LLM.DTOs;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;

namespace PencilCase.Web.Services.LLM;

public class LlmApi : ILlmApi
{
    private readonly HttpClient _httpClient;

    public LlmApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("LlmAPI");
    }
    
    public async Task<IEnumerable<LlmMessage>>  InvokeLlmAgentAsync(IEnumerable<LlmMessage> llmChat)
    {
        var response = await _httpClient.PostAsJsonAsync("agent/invoke", llmChat);
        response.EnsureSuccessStatusCode();
        var responseContents = await response.Content.ReadFromJsonAsync<List<LlmMessage>>() ?? new List<LlmMessage>();
        return responseContents;
    }

    public async Task AddRagDocumentsAsync(IEnumerable<RagDocument> documents)
    {
        var response = await _httpClient.PostAsJsonAsync("rag", documents);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<RagDocument>> QueryRagDocumentsAsync(string query, List<Guid> parentIds, uint? nResults=3)
    {
        var request = new QueryRequest
        {
            Query = query,
            ParentIds = parentIds,
            NResults = nResults ?? 3
        };
        var response = await _httpClient.PostAsJsonAsync("rag/search", request);
        
        response.EnsureSuccessStatusCode();
        var responseContents = await response.Content.ReadFromJsonAsync<List<RagDocument>>() ?? new List<RagDocument>();
        return responseContents;
    }

    public Task DeleteRagDocumentsAsync(IEnumerable<RagDocument> documents)
    {
        throw new NotImplementedException();
    }
}
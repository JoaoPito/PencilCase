using System.Net.Http.Json;
using PencilCase.Shared.DTOs.Requests.Sources;
using PencilCase.Shared.DTOs.Responses.Parser;

namespace PencilCase.Web.Client.Services.Sources;

public class SourcesApi: ISourcesApi
{
    private readonly HttpClient _httpClient;

    public SourcesApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("SourcesAPI");
    }
    
    public async Task<Guid> StartUpload(UploadSourceRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync<UploadSourceRequest>("", request);
        if(!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to upload source: {response.StatusCode}");
        var jobId = await response.Content.ReadFromJsonAsync<Guid>();
        return jobId;
    }

    public async Task<JobStatusResponse> GetUploadStatus(Guid jobId)
    {
        var response = await _httpClient.GetFromJsonAsync<JobStatusResponse>($"{jobId}");
        if(response == null) 
            throw new HttpRequestException("Job status response is null");
        return response;
    }
}
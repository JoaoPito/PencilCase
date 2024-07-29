using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PencilCase.Web.Components;

namespace PencilCase.Web.Services;

public class RoadmapAPI
{
    private const String InvokeEndpoint = "roadmap/invoke";
    private readonly HttpClient _httpClient;

    public RoadmapAPI(IHttpClientFactory httpFactory)
    {
        this._httpClient = httpFactory.CreateClient("API");
    }

    public async Task<Roadmap> Generate(string topic)
    {
        using StringContent jsonRequestContent = GetGenerationRequest(topic);
        
        var response = await _httpClient.PostAsync(InvokeEndpoint, jsonRequestContent);

        response.EnsureSuccessStatusCode();
        var responseObj = await JsonSerializer.DeserializeAsync<SGApiResponse>(await response.Content.ReadAsStreamAsync());

        if (responseObj is null)
        {
            throw new NullReferenceException();
        }

        return MapResponseToRoadmap(responseObj);
    }

    public StringContent GetGenerationRequest(string topic)
    {
        return new(
            JsonSerializer.Serialize(new
            {
                input = new
                {
                    topic = topic
                }
            }),
            Encoding.UTF8,
            "application/json"
        );
    }

    public Roadmap MapResponseToRoadmap(SGApiResponse response)
    {
        return new Roadmap(response.Output);
    }
}
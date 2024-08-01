using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PencilCase.Shared.Models;
using PencilCase.Web.Models;
using PencilCase.Web.Models.Responses.StudyGuide;

namespace PencilCase.Web.Services;

public class FragmentApi
{
    private readonly HttpClient _httpClient;

    public FragmentApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("API");
    }
    public async Task<List<FragmentGenerator>?> GetAllGenerators()
    {
        var responses = await _httpClient.GetFromJsonAsync<List<FragmentResponse>>("\\");
        if (responses == null)
            throw new NullReferenceException("Response is null!");
        return MapResponsesToInvocableFragments(responses);
    }

    public async Task<Fragment> GenerateFragment(FragmentGenerator fragmentGenerator, string topic){
        var requestContent = GetGenerationRequest(topic);
        var response = await _httpClient.PostAsync(fragmentGenerator.Endpoint, requestContent);
        response.EnsureSuccessStatusCode();
        var responseObj = await JsonSerializer.DeserializeAsync<InvokeResponse>(await response.Content.ReadAsStreamAsync());

        if (responseObj is null)
        {
            throw new NullReferenceException();
        }

        return new Fragment{
            Name = fragmentGenerator.Name, 
            Content = responseObj.Output
        };
    }
    
    private FragmentGenerator MapResponseToInvocableFragment(FragmentResponse response)
    {
        return new FragmentGenerator{
            Name = response.Name, 
            Description = response.Description,
            Endpoint = response.Endpoint
        };
    }

    private List<FragmentGenerator> MapResponsesToInvocableFragments(List<FragmentResponse> responses)
    {
        return responses.Select(r => MapResponseToInvocableFragment(r)).ToList();
    }

    private StringContent GetGenerationRequest(string topic)
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

    
}
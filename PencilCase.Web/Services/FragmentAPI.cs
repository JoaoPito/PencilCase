using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PencilCase.Shared.Models;
using PencilCase.Shared.Models.StudyGuide;
using PencilCase.Web.Components;
using PencilCase.Web.Models;
using PencilCase.Web.Models.Responses;

namespace PencilCase.Web.Services;

public class FragmentAPI
{
    private HttpClient _httpClient;
    private List<FragmentEndpoint>? _fragments = null;

    public FragmentAPI(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("API");
    }
    public async Task<List<FragmentEndpoint>?> GetAvailableFragments(){
        if(_fragments == null || _fragments.Count == 0)
            _fragments = await _httpClient.GetFromJsonAsync<List<FragmentEndpoint>>("\\");
        return _fragments;
    }

    public async Task<Fragment> GenerateFragment(string name, string topic){
        await GetAvailableFragments();
        EnsureHasFragments();
        var requestedFragment = GetFragmentEndpointBy(f => f.Name == name);

        var requestContent = GetGenerationRequest(topic);
        var response = await _httpClient.PostAsync(requestedFragment.Endpoint, requestContent);
        response.EnsureSuccessStatusCode();
        var responseObj = await JsonSerializer.DeserializeAsync<SGApiResponse>(await response.Content.ReadAsStreamAsync());

        if (responseObj is null)
        {
            throw new NullReferenceException();
        }

        return MapResponseToFragment(requestedFragment, responseObj);
    }

    private void EnsureHasFragments(){
        if(_fragments == null || _fragments.Count == 0)
            throw new ArgumentException("Could not load any fragments");
    }

    private FragmentEndpoint GetFragmentEndpointBy(Func<FragmentEndpoint, bool> criteria){
        EnsureHasFragments();
        var requestedFragment = _fragments.FirstOrDefault(criteria);

        if(requestedFragment == null)
            throw new ArgumentException("Could not find requested fragment");
        return requestedFragment;
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

    private Fragment MapResponseToFragment(FragmentEndpoint fragment,SGApiResponse response)
    {
        return new Fragment(fragment.Name, response.Output);
    }
}
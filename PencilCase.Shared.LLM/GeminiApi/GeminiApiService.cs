using System.Dynamic;
using System.Net.Http.Json;
using PencilCase.Shared.LLM.Models;

namespace PencilCase.Shared.LLM.GeminiApi;

public class GeminiApiService : ILlmApiService
{
    private const string GeminiApiUrl = "\"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=$GOOGLE_API_KEY\"";
    private readonly string _model;
    private readonly HttpClient _httpClient;

    public GeminiApiService(IHttpClientFactory httpClientFactory, string model="gemini-1.5-flash")
    {
        _model = model;
        _httpClient = httpClientFactory.CreateClient("GeminiApi");
    }

    public async Task<List<Message>> GenerateContent(List<Message> userPrompt)
    {
        var request = MapMessageListToRequest(userPrompt);
        var response = await _httpClient.PostAsJsonAsync<GeminiApiRequest>($"/{_model}:generateContent?key=$GOOGLE_API_KEY", request);
        if(!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Error '{response.StatusCode}' while generating content with gemini model.");

        var responseContents = await response.Content.ReadFromJsonAsync<GeminiApiResponse>();
        if(responseContents is null)
            throw new ArgumentException($"Reponse from Gemini API is empty!");
        
        return MapResponseToMessageList(responseContents);
    }

    private GeminiApiRequest MapMessageListToRequest(List<Message> userPrompt)
    {
        var request = new GeminiApiRequest();
        foreach (var prompt in userPrompt)
        {
            request.Contents = request.Contents.Append(MapMessageToRequestContent(prompt));
        }
        return request;
    }
    
    private GeminiApiRequestContent MapMessageToRequestContent(Message msg)
    {
        var request = new GeminiApiRequestContent()
        {
            Role = msg.Role,
            Parts = new List<GeminiApiRequestPart>()
            {
                new GeminiApiRequestPart()
                {
                    Text = msg.Content
                }
            }
        };
        return request;
    }

    private List<Message> MapResponseToMessageList(GeminiApiResponse geminiApiResponse)
    {
        var messages = new List<Message>();
        foreach (var candidate in geminiApiResponse.Candidates)
        {
            if (candidate.Content == null)
                continue;
            
            var candidateText = string.Empty;
            if (candidate.Content != null && candidate.Content.Parts.Any())
            {
                candidateText = candidate.Content.Parts.First().Text;
            }
            
            messages.Add(new Message()
            {
                Role = candidate.Content!.Role,
                Content = candidateText
            });
        }

        return messages;
    }
}
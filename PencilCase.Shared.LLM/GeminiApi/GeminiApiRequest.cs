using System.Collections;
using System.Text.Json.Serialization;

namespace PencilCase.Shared.LLM.GeminiApi;

public class GeminiApiRequest
{
    [JsonPropertyName("contents")]
    public IEnumerable<GeminiApiRequestContent> Contents { get; set; } = new List<GeminiApiRequestContent>();
}

public class GeminiApiRequestContent
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    [JsonPropertyName("parts")]
    public IEnumerable<GeminiApiRequestPart> Parts { get; set; } = new List<GeminiApiRequestPart>();
}

public class GeminiApiRequestPart
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
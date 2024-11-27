using System.Text.Json.Serialization;

namespace PencilCase.LLM.Agents.Providers.Gemini;

public class GeminiApiRequest
{
    [JsonPropertyName("system_instruction")]
    public SystemInstruction? SystemInstruction { get; set; }
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

public class SystemInstruction
{
    [JsonPropertyName("parts")]
    public GeminiApiRequestPart? Parts { get; set; }
}
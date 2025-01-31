using System.Text.Json.Serialization;

namespace PencilCase.Shared.DTOs.Responses.Parser;

public class ParserResponse
{
    [JsonPropertyName("chunks")] public List<String> Chunks { get; set; } = new();
}
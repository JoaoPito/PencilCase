using System.Text.Json.Serialization;

namespace PencilCase.Shared.Models.LLM.Parser;

public class ParserFile
{
    [JsonPropertyName("filename")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("file_contents")]
    public string Contents { get; set; } = string.Empty;
}
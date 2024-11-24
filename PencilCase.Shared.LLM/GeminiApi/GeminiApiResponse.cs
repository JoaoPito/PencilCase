using System.Text.Json.Serialization;

namespace PencilCase.Shared.LLM.GeminiApi;
using System.Collections.Generic;

public class GeminiApiResponse
{
    [JsonPropertyName("candidates")]
    public List<Candidate> Candidates { get; set; } = new List<Candidate>();
    [JsonPropertyName("usageMetadata")]
    public UsageMetadata? UsageMetadata { get; set; }
    [JsonPropertyName("modelVersion")]
    public string ModelVersion { get; set; } = string.Empty;
}

public class Part
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class Content
{
    [JsonPropertyName("parts")] 
    public List<Part> Parts { get; set; } = new();
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}

public class CitationSource
{
    [JsonPropertyName("startIndex")]
    public int? StartIndex { get; set; }
    [JsonPropertyName("endIndex")]
    public int? EndIndex { get; set; }
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }
}

public class CitationMetadata
{
    [JsonPropertyName("citationSources")]
    public List<CitationSource>? CitationSources { get; set; }
}

public class Candidate
{
    [JsonPropertyName("content")]
    public Content Content { get; set; }
    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; set; }
    [JsonPropertyName("citationMetadata")]
    public CitationMetadata? CitationMetadata { get; set; }
    [JsonPropertyName("avgLogprobs")]
    public double? AvgLogprobs { get; set; }
}

public class UsageMetadata
{
    [JsonPropertyName("promptTokenCount")]
    public int? PromptTokenCount { get; set; }
    [JsonPropertyName("candidatesTokenCount")]
    public int? CandidatesTokenCount { get; set; }
    [JsonPropertyName("totalTokenCount")]
    public int? TotalTokenCount { get; set; }
}


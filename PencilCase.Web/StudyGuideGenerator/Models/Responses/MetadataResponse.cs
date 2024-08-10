using System.Text.Json.Serialization;

namespace PencilCase.Web.StudyGuideGenerator.Models.Responses;

public record MetadataResponse(
    [property: JsonPropertyName("run_id")] string RunId, 
    [property: JsonPropertyName("feedback_tokens")] ICollection<Object> FeedbackTokens);
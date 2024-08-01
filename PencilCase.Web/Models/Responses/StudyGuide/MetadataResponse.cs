using System.Text.Json.Serialization;

namespace PencilCase.Web.Models.Responses.StudyGuide;

public record MetadataResponse(
    [property: JsonPropertyName("run_id")] string RunId, 
    [property: JsonPropertyName("feedback_tokens")] ICollection<Object> FeedbackTokens);
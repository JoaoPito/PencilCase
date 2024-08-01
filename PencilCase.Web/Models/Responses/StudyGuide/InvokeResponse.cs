using System.Text.Json.Serialization;

namespace PencilCase.Web.Models.Responses.StudyGuide;

public record InvokeResponse(
    [property: JsonPropertyName("output")] String Output, 
    [property: JsonPropertyName("metadata")] MetadataResponse MetadataResponse);
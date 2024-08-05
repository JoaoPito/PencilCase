using System.Text.Json.Serialization;

namespace PencilCase.Web.StudyGuide.Models.Responses;

public record InvokeResponse(
    [property: JsonPropertyName("output")] String Output, 
    [property: JsonPropertyName("metadata")] MetadataResponse MetadataResponse);
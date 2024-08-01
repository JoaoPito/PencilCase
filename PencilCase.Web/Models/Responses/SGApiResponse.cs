using System.Text.Json.Serialization;

namespace PencilCase.Web.Models.Responses;

public record SGApiResponse(
    [property: JsonPropertyName("output")] String Output, 
    [property: JsonPropertyName("metadata")] SGApiMetadata Metadata);
using System.Text.Json.Serialization;

namespace PencilCase.Web.Components;

public record SGApiResponse(
    [property: JsonPropertyName("output")] String Output, 
    [property: JsonPropertyName("metadata")] SGApiMetadata Metadata);
using System.Text.Json.Serialization;

namespace PencilCase.Web.Pages.StudyGuideGenerator.Models.Responses;

public record FragmentResponse([property: JsonPropertyName("name")] String Name,
    [property: JsonPropertyName("description")] String Description,
    [property: JsonPropertyName("endpoint")] String Endpoint);
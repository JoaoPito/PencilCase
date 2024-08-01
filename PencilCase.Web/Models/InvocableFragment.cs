using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PencilCase.Web.Models;

public record InvocableFragment(
    [property: JsonPropertyName("name")] String Name,
    [property: JsonPropertyName("description")] String Description,
    [property: JsonPropertyName("endpoint")] String Endpoint);
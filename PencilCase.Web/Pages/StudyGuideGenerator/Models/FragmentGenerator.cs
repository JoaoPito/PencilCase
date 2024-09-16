namespace PencilCase.Web.Pages.StudyGuideGenerator.Models;

public record FragmentGenerator
{
    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public string Endpoint { get; set; } = String.Empty;
}
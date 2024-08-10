namespace PencilCase.Web.StudyGuideGenerator.Models;

public record FragmentToolbarActions
{
    public Action<String>? OnRegenerate { get; set; } 
    public Action<String>? OnDelete { get; set; } 
}
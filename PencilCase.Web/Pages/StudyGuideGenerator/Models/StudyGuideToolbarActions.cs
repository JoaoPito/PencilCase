namespace PencilCase.Web.Pages.StudyGuideGenerator.Models;

public record StudyGuideToolbarActions
{
    public Action<String>? OnRegenerate { get; set; } 
    public Action<String>? OnDelete { get; set; } 
    public Action<String>? OnShare { get; set; } 

}
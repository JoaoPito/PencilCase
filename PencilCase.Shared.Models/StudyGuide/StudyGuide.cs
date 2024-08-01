namespace PencilCase.Shared.Models;

public record StudyGuide
{
    public IEnumerable<Fragment> Fragments = new List<Fragment>();
    public DateTime CreatedIn = DateTime.Now;
    public DateTime LastModified = DateTime.Now;
    public DateTime LastOpened = DateTime.Now;
    public string Topic = string.Empty;
}
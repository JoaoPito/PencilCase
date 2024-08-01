namespace PencilCase.Shared.Models;

public record Fragment
{
    public DateTime Created = DateTime.UtcNow;
    public DateTime LastModified = DateTime.UtcNow;
    public String Name = String.Empty;
    public String Content = String.Empty;
}
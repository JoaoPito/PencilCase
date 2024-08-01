namespace PencilCase.Shared.Models;

public record Fragment
{
    DateTime created = DateTime.UtcNow;
    DateTime lastModified = DateTime.UtcNow;
    String name = String.Empty;
    String content = String.Empty;

}
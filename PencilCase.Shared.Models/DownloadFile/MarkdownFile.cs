namespace PencilCase.Shared.Models.DownloadFile;

public class MarkdownFile : TextFile
{
    public MarkdownFile(String name, String contents) : base(name, contents, "md")
    {
        
    }
}
namespace PencilCase.Shared.Models.DownloadFile;

public abstract class TextFile
{
    public string Name { get; }
    public string Contents { get; }
    public string Extension { get; }

    public TextFile(String name, String contents, String extension)
    {
        this.Name = name;
        this.Contents = contents;
        this.Extension = extension;
    }
}
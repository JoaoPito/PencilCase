namespace PencilCase.Shared.Files.Types;

public class BinaryFile
{
    public string Name { get; }
    public byte[] Contents { get; }
    public string Extension { get; }

    public BinaryFile(String name, byte[] contents, String extension)
    {
        this.Name = name;
        this.Contents = contents;
        this.Extension = extension;
    }

    public String GetFileName(){
        return Name + Extension;
    }

    public byte[] GetContents(){
        return Contents;
    }
}
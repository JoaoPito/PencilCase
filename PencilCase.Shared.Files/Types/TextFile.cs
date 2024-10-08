using System.Text;

namespace PencilCase.Shared.Files.Types;

public abstract class TextFile : BinaryFile
{
    public TextFile(String name, String contents, String extension) : base(name, Encoding.UTF8.GetBytes(contents), extension)
    {
        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PencilCase.Shared.Files.Types;

public class PdfFile : BinaryFile
{
    public PdfFile(String name, byte[] contents) : base(name, contents, ".pdf")
    {

    }
}
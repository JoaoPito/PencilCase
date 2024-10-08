using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PencilCase.Shared.Files.Types;

public class PdfFile : TextFile
{
    public PdfFile(String name, String contents) : base(name, contents, ".pdf")
    {

    }
}
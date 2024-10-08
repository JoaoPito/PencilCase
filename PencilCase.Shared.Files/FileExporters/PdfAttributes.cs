using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PencilCase.Shared.Files.FileExporters;

public abstract class PdfAttributes
{
    public abstract XUnit TitleSize { get; }
    public abstract XUnit BodySize { get; }
    public abstract XFont TitleFont { get; }
    public abstract XFont BodyFont { get; }

    public abstract XUnit Margins { get; }

    public abstract void SetupPage(PdfPage page);
}
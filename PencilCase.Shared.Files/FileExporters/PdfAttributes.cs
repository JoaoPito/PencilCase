using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PencilCase.Shared.Files.FileExporters;

public abstract class PdfAttributes
{
    public abstract int TitleFontSize { get; }
    public abstract int BodyFontSize { get; }
    public abstract string TitleFontName { get; }
    public abstract string BodyFontName { get; }
    public abstract XFont TitleFont { get; }
    public abstract XFont BodyFont { get; }

    public abstract XUnit Margins { get; }

    public abstract void SetupPage(PdfPage page);
}
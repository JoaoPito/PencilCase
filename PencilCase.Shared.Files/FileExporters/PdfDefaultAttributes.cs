using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PencilCase.Shared.Files.FileExporters;

public class PdfDefaultAttributes : PdfAttributes
{
    public override int TitleFontSize => 20;
    public override int BodyFontSize => 12;
    public override string TitleFontName => "Noto Sans";
    public override string BodyFontName => "Noto Sans";

    public override XFont TitleFont => new XFont(BodyFontName, BodyFontSize, XFontStyleEx.Bold);
    public override XFont BodyFont => new XFont(BodyFontName, BodyFontSize, XFontStyleEx.Regular);

    public override XUnit Margins => XUnit.FromCentimeter(2.5);

    public override void SetupPage(PdfPage page)
    {
        page.Size = PdfSharp.PageSize.A4;
        page.Orientation = PdfSharp.PageOrientation.Portrait;
    }
}
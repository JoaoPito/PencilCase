using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;

namespace PencilCase.Shared.Files.FileExporters;

public class PdfDefaultAttributes : PdfAttributes
{
    public override int TitleFontSize => 20;
    public override int BodyFontSize => 12;
    public override string TitleFontName => "Noto Sans";
    public override string BodyFontName => "Noto Sans";

    public override XFont TitleFont => new XFont(BodyFontName, TitleFontSize, XFontStyleEx.Bold);
    public override XFont BodyFont => new XFont(BodyFontName, BodyFontSize, XFontStyleEx.Regular);

    public override XUnit Margins => XUnit.FromCentimeter(2.5);

    private const string footerText = "made with pencil-case.com";
    private XFont footerFont => new XFont(BodyFontName, 8, XFontStyleEx.Bold);

    public override void SetupPage(PdfPage page, XGraphics gfx)
    {
        page.Size = PdfSharp.PageSize.A4;
        page.Orientation = PdfSharp.PageOrientation.Portrait;
        AddFooter(page, gfx);
    }

    private void AddFooter(PdfPage page, XGraphics gfx)
    {
        var tf = new XTextFormatter(gfx);
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString(footerText, footerFont, XBrushes.Black, 
            new XRect(Margins, 
                page.Height - (Margins / 2) - 8, 
                page.Width, 
                page.Height - Margins), 
            XStringFormats.TopLeft);
    }
}
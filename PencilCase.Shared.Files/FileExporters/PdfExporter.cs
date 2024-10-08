using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PencilCase.Shared.Files.Types;
using PencilCase.Shared.Models;
using System.IO;

namespace PencilCase.Shared.Files.FileExporters;

public class PdfExporter : FileExporter
{
    public PdfExporter()
    {

    }

    public override BinaryFile ExportStudyGuide(StudyGuide studyGuide)
    {
        PdfDocument document = new PdfDocument();
        AddMetadata(document, studyGuide);
    
        foreach(var fragment in studyGuide.Fragments)
        {
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            SetupPage(page);

            AddFragmentToPage(fragment, page, gfx);
        }

        using (MemoryStream stream = new MemoryStream())
        {
            document.Save(stream);
            return new PdfFile(studyGuide.Topic, stream.ToArray());
        }
    }

    private void SetupPage(PdfPage page)
    {
        page.Size = PdfSharp.PageSize.A4;
        page.Orientation = PdfSharp.PageOrientation.Portrait;
    }

    private void AddMetadata(PdfDocument document, StudyGuide studyGuide)
    {
        document.Info.Title = studyGuide.Topic + " - Study Guide";
        document.Info.Author = "pencil-case.com";
        document.Info.Subject = studyGuide.Topic;
    }

    private void AddFragmentToPage(Fragment fragment, PdfPage page, XGraphics gfx)
    {
        AddTitleToPage(fragment.Name, page, gfx);
        AddContentToPage(fragment.Content, page, gfx);
    }

    private void AddTitleToPage(string Title, PdfPage page, XGraphics gfx)
    {
        var titleFont = new XFont("Noto Sans", 20, XFontStyleEx.Regular);
        gfx.DrawString(Title, titleFont, XBrushes.Black, 
            new XRect(10, 0, page.Width - 10, page.Height), XStringFormats.TopLeft);
    }
    private void AddContentToPage(string Content, PdfPage page, XGraphics gfx)
    {
        var contentFont = new XFont("Noto Sans", 14, XFontStyleEx.Regular);
        gfx.DrawString(Content, contentFont, XBrushes.Black, 
            new XRect(10, 25, page.Width - 10, page.Height), XStringFormats.TopLeft);
    }
}
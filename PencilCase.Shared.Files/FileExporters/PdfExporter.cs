using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PencilCase.Shared.Files.Types;
using PencilCase.Shared.Models;
using System.IO;

namespace PencilCase.Shared.Files.FileExporters;

public class PdfExporter : FileExporter
{
    private readonly PdfAttributes attributes;

    public PdfExporter(PdfAttributes attributes)
    {
        this.attributes = attributes;
    }

    public override BinaryFile ExportStudyGuide(StudyGuide studyGuide)
    {
        PdfDocument document = new PdfDocument();
        AddMetadata(document, studyGuide);
    
        foreach(var fragment in studyGuide.Fragments)
        {
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            attributes.SetupPage(page, gfx);

            AddFragmentToPage(fragment, page, gfx);
        }

        using (MemoryStream stream = new MemoryStream())
        {
            document.Save(stream);
            return new PdfFile(studyGuide.Topic, stream.ToArray());
        }
    }

    private void AddMetadata(PdfDocument document, StudyGuide studyGuide)
    {
        document.Info.Title = (studyGuide.Topic == "") ? "Empty Study Guide" : studyGuide.Topic + " - Study Guide";
        document.Info.Author = "pencil-case.com";
        document.Info.Subject = (studyGuide.Topic == "") ? "Empty Study Guide" : studyGuide.Topic;
    }

    private void AddFragmentToPage(Fragment fragment, PdfPage page, XGraphics gfx)
    {
        var textFormatter = new XTextFormatter(gfx);
        AddTitleToPage(fragment.Name, page, textFormatter);
        AddBodyToPage(fragment.Content, page, textFormatter);
    }

    private void AddTitleToPage(string Title, PdfPage page, XTextFormatter tf)
    {
        var titleFont = attributes.TitleFont;
        var titleRect = new XRect(attributes.Margins,
                            attributes.Margins, 
                            page.Width - (2 * attributes.Margins), 
                            page.Height - (2 * attributes.Margins));

        tf.DrawString(Title, titleFont, XBrushes.Black, 
            titleRect, XStringFormats.TopLeft);
    }
    private void AddBodyToPage(string Content, PdfPage page, XTextFormatter tf)
    {
        var contentFont = attributes.BodyFont;
        var bodyRect = new XRect(attributes.Margins,
                            attributes.Margins + (2 * attributes.TitleFontSize), 
                            page.Width - (2 * attributes.Margins), 
                            page.Height - (2 * attributes.Margins) - attributes.TitleFontSize);

        tf.DrawString(Content, contentFont, XBrushes.Black, 
            bodyRect, XStringFormats.TopLeft);
    }
}
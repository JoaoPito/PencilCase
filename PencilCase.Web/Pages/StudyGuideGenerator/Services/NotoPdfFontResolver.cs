using PdfSharp.Drawing;
using System.IO;
using PdfSharp.Fonts;

namespace PencilCase.Web.Pages.StudyGuideGenerator.Services;

public class NotoPdfFontResolver : IFontResolver
{
    private readonly byte[] _fontData;

    public NotoPdfFontResolver(byte[] fontData)
    {
        _fontData = fontData;
    }

    public byte[]? GetFont(string faceName)
    {
        return faceName switch
        {
            "NotoSansRegular" => _fontData,
            _ => null
        };
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (familyName.ToLower() == "noto sans")
        {
            return new FontResolverInfo("NotoSansRegular");
        }

        return null;
    }
}
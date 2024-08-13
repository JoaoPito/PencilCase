using PencilCase.Shared.Files.Types;
using PencilCase.Shared.Models;

namespace PencilCase.Shared.Files.FileExporters;

public class MarkdownExporter : TextFileExporter
{
    public override TextFile ExportStudyGuide(StudyGuide studyGuide){
        var filename = studyGuide.Topic + studyGuide.LastOpened
                                            .ToLocalTime()
                                            .ToString("dd_MM_yyyy");
        var file_contents = FormatTitle(studyGuide.Topic);

        foreach(var fragment in studyGuide.Fragments){
            var sectionTitle = FormatSubtitle(fragment.Name);
            var sectionContent = $"{fragment.Content}\n";
            file_contents += sectionTitle + sectionContent;
        }

        MarkdownFile file = new MarkdownFile(filename, file_contents);
        return file;
    }

    protected String CapitalizeText(String text){
        if (string.IsNullOrEmpty(text))
            return text;
        string[] words = text.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
        }
        return string.Join(" ", words);
    }

    protected String FormatTitle(String title){
        return $"# {CapitalizeText(title)}\n";
    }

    protected String FormatSubtitle(String subtitle) {
        return $"## {CapitalizeText(subtitle)}\n";
    }
}
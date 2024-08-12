using PencilCase.Shared.Files.Types;
using PencilCase.Shared.Models;

namespace PencilCase.Shared.Files.FileExporters;

public class MarkdownExporter : TextFileExporter
{
    public override TextFile ExportStudyGuide(StudyGuide studyGuide){
        var filename = studyGuide.Topic + studyGuide.LastOpened
                                            .ToLocalTime()
                                            .ToString("dd_MM_yyyy");
        var file_contents = $"# {studyGuide.Topic}";

        foreach(var fragment in studyGuide.Fragments){
            var sectionTitle = $"## {fragment.Name}\n";
            var sectionContent = $"{fragment.Content}\n";
            file_contents += sectionTitle + sectionContent;
        }

        MarkdownFile file = new MarkdownFile(filename, file_contents);
        return file;
    }
}
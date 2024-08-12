using PencilCase.Shared.Models;
using PencilCase.Shared.Models.DownloadFile;

namespace PencilCase.Shared.Files;

public abstract class TextFileExporter
{
    public abstract TextFile ExportStudyGuide(StudyGuide content);
}
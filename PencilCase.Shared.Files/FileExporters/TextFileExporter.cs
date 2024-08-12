using PencilCase.Shared.Files.Types;
using PencilCase.Shared.Models;

namespace PencilCase.Shared.Files;

public abstract class TextFileExporter
{
    public abstract TextFile ExportStudyGuide(StudyGuide content);
}
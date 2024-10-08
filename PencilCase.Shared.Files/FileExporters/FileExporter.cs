using PencilCase.Shared.Files.Types;
using PencilCase.Shared.Models;

namespace PencilCase.Shared.Files;

public abstract class FileExporter
{
    public abstract BinaryFile ExportStudyGuide(StudyGuide content);
}
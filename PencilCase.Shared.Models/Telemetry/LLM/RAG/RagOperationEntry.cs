namespace PencilCase.Shared.Models.Telemetry.LLM.RAG;

public class RagOperationEntry
{
    public uint Id { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public RagOperationType OperationType { get; set; }
    public uint TokensUsed { get; set; }
    public uint UnitsUsed { get; set; }
    public uint? FileSizeBytes { get; set; }
}
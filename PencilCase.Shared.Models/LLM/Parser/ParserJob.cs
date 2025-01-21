namespace PencilCase.Shared.Models.LLM.Parser;

public class ParserJob
{
    public enum JobStatus
    {
        Accepted = 0,
        Processing = 1,
        Completed = 2,
        Failed = -1,
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public JobStatus Status { get; set; } = JobStatus.Accepted;
    public string? StatusMsg { get; set; }
    public ParserFile File { get; set; } = new();
    public Guid ParentBlockId { get; set; }
}
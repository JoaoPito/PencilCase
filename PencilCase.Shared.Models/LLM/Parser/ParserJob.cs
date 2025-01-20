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

    public JobStatus Status { get; set; }
    public string? StatusMsg { get; set; }
    public ParserFile ParserFile { get; set; } = new();
}
using PencilCase.Shared.Models.LLM.Parser;

namespace PencilCase.Shared.DTOs.Requests.Sources;

public class JobStatusResponse
{
    public ParserJob.JobStatus StatusCode { get; set; }
    public string? StatusMsg { get; set; }
}
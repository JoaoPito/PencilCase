using PencilCase.Shared.Models.LLM.Parser;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Shared.DTOs.Responses.Parser;

public class JobStatusResponse
{
    public ParserJob.JobStatus StatusCode { get; set; }
    public string? StatusMsg { get; set; }
    public Guid? DocumentBlockId { get; set; }
}
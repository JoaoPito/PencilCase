using PencilCase.Shared.Models.LLM.Parser;

namespace PencilCase.LLM.Parser.Services;

public interface IParserProducerService
{
    public Task SubmitJobAsync(ParserJob job);
    public Task<ParserJob> GetJobAsync(Guid jobId);
}
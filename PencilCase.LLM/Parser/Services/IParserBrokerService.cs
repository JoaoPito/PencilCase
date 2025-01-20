using PencilCase.Shared.Models.LLM.Parser;

namespace PencilCase.LLM.Parser.Services;

public interface IParserBrokerService
{
    public Task<Guid> SubmitJobAsync(ParserJob job);
    public Task<ParserJob> GetJobAsync(Guid jobId);
}
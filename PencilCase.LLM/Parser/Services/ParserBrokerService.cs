using PencilCase.Shared.Models.LLM.Parser;

namespace PencilCase.LLM.Parser.Services;

public class ParserBrokerService: IParserBrokerService
{
    public Task<Guid> SubmitJobAsync(ParserJob job)
    {
        throw new NotImplementedException();
    }

    public Task<ParserJob> GetJobAsync(Guid jobId)
    {
        throw new NotImplementedException();
    }
}
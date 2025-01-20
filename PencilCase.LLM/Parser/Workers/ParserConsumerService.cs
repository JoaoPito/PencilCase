using Microsoft.Extensions.Hosting;

namespace PencilCase.LLM.Parser.Workers;

public class ParserConsumerService: BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
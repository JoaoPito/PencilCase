using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PencilCase.Shared.Models.LLM.Parser;
using StackExchange.Redis;

namespace PencilCase.LLM.Parser.Services;

public class ParserProducerService(IConfiguration configuration, IConnectionMultiplexer redis) : IParserProducerService
{
    private const string JobChannelConfig = "ParserBroker:JobChannel";
    private readonly string _jobChannel = configuration.GetValue<string>(JobChannelConfig) ?? "parserjobs";

    public async Task SubmitJobAsync(ParserJob job)
    {
        await SetJobStatusAsync(job);
        await PublishJobToAcceptedChannel(job);
    }

    public async Task<ParserJob> GetJobAsync(Guid jobId)
    {
        var db = redis.GetDatabase();
        
        var job = await db.StringGetAsync($"{_jobChannel}:{jobId}");
        if (job.HasValue) return JsonSerializer.Deserialize<ParserJob>(job!)!;
        
        throw new ArgumentException($"No job found with id {jobId}");
    }

    private async Task SetJobStatusAsync(ParserJob job)
    {
        var db = redis.GetDatabase();
        await db.StringSetAsync(
            $"{_jobChannel}:{job.Id}",
            JsonSerializer.Serialize(job), 
            TimeSpan.FromMinutes(60));
    }

    private async Task PublishJobToAcceptedChannel(ParserJob job)
    {
        var subscriber = redis.GetSubscriber();
        await subscriber.PublishAsync(
            new RedisChannel($"{_jobChannel}:accepted", RedisChannel.PatternMode.Literal),
            JsonSerializer.Serialize(job),
            CommandFlags.FireAndForget);
    }
}
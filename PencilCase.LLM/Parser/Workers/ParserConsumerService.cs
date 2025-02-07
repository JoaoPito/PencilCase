using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PencilCase.LLM.VectorDb;
using PencilCase.Shared.Data.Database;
using PencilCase.Shared.DTOs.Responses.Parser;
using PencilCase.Shared.Models.LLM.Parser;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;
using StackExchange.Redis;

namespace PencilCase.LLM.Parser.Workers;

public class ParserConsumerService(
    ILogger<ParserConsumerService> logger,
    IConnectionMultiplexer redis,
    IServiceProvider serviceProvider,
    IConfiguration configuration)
    : BackgroundService
{
    private const string JobChannelConfig = "ParserBroker:JobChannel";
    private readonly string _jobChannel = configuration.GetValue<string>(JobChannelConfig) ?? "parserjobs";
    private readonly string _acceptedJobSubChannel = "accepted";

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    
    private const string ParsingDocumentStatusMsg = "Studying hard... \ud83e\udd14\n";
    private const string StoringDocumentStatusMsg = "Memorizing... \ud83e\udd13\n";
    private const string JobCompletedStatusMsg = "Finished! \ud83d\ude03\n";
    
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Worker");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Worker");
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Executing Worker");
        var channel = await redis.GetSubscriber().SubscribeAsync(
            new RedisChannel($"{_jobChannel}:{_acceptedJobSubChannel}", RedisChannel.PatternMode.Auto)
            );
        channel.OnMessage(async (msg) =>
        {
            try
            {
                if ((string?) msg.Message != null && msg.Message.HasValue)
                {
                    var job = JsonSerializer.Deserialize<ParserJob>(msg.Message!);
                    if (job is null) throw new NullReferenceException("Job is null after deserialization");
                    
                    await HandleJob(job);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        });
    }

    private async Task HandleJob(ParserJob job)
    {
        // Update Job Status
        job.Status = ParserJob.JobStatus.Processing;
        job.StatusMsg = ParsingDocumentStatusMsg;
        await UpdateJob(job);
        
        logger.LogInformation($"Processing job {job.Id}");
        
        // Create new Block for entire Document
        var docBlock = await CreateDocBlock(job.File.Name, job.ParentBlockId);

        // Send document to Parser API
        // Wait for chunks
        var chunks = await TrySendFileToParserOrFailJob(job);
        
        job.StatusMsg = StoringDocumentStatusMsg;
        await UpdateJob(job);
        
        logger.LogInformation($"Got {chunks.Count} chunks for job {job.Id}. Persisting them.");
        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var id = Guid.NewGuid();
            
            // Store results in VectorDB
            await StoreChunkInVectorDb(
                id,
                chunk,
                docBlock.Id);
            
            // Store results in BlocksDB
            await StoreChunkInBlocksDb(
                id,
                chunk,
                docBlock,
                i);
        }
        
        // Update Job Status
        job.Status = ParserJob.JobStatus.Completed;
        job.SourceBlock = docBlock;
        job.StatusMsg = JobCompletedStatusMsg;
        await UpdateJob(job);
    }

    private async Task<Block> CreateDocBlock(string fileName, Guid jobParentBlockId)
    {
        // Use BlocksDal scoped service
        using var scope = _serviceProvider.CreateScope();
        var dal = scope.ServiceProvider.GetRequiredService<IBlocksDal>();
        // Create new block
        var block = new Block()
        {
            Name = fileName,
            Type = BlockType.Source,
            ParentId = jobParentBlockId,
        };
        var blockProperties = new BlockProperties()
        {
            Parent = block,
            ParentId = block.Id
        };
        block.Properties = blockProperties;
            
        // Add new block
        await dal.Add(block);
        return block;
    }

    async Task<List<String>> TrySendFileToParserOrFailJob(ParserJob job)
    {
        try
        {
            return await SendFileToParser(job.File);
        }
        catch (Exception e)
        {
            job.Status = ParserJob.JobStatus.Failed;
            job.StatusMsg = e.Message;
            await UpdateJob(job);
            throw;
        }
    }

    async Task<List<String>> SendFileToParser(ParserFile file)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var llmApiClient = scope
                .ServiceProvider
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient("LLMApi-FileParser");

            var response = await llmApiClient.PostAsJsonAsync<ParserFile>("v1/file", file);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Error while submitting file with name {file.Name} to parser on LLM API");
            var parserResponse = await response.Content.ReadFromJsonAsync<ParserResponse>();
            if (parserResponse is null)
                throw new NullReferenceException("Parser response is null");
            
            return parserResponse.Chunks;
        }
    }
    
    async Task UpdateJob(ParserJob job)
    {
        var db = redis.GetDatabase();
        await db.StringSetAsync($"{_jobChannel}:{job.Id}", 
            JsonSerializer.Serialize(job));
    }

    async Task StoreChunkInVectorDb(Guid id, string chunk, Guid parentBlock)
    {
        // Create RagDocument
        var doc = new RagDocument()
        {
            ParentId = parentBlock,
            Id = id,
            Content = chunk,
        };
        
        // Use scoped IRagService
        using (var scope = _serviceProvider.CreateScope())
        {
            // Store RagDocument into IRagService
            var rag = scope.ServiceProvider.GetRequiredService<IRagService>();
            await rag.AddChunks(new List<RagDocument>() { doc });
        }
    }
    
    async Task StoreChunkInBlocksDb(Guid id, string chunk, Block parent, int order)
    {
        // Create Block
        var block = new Block()
        {
            Id = id,
            ParentId = parent.Id,
            //Parent = parent,
            Name = chunk,
            Type = BlockType.Cell,
        };
        var properties = new BlockProperties()
        {
            Order = order,
            Parent = block,
            ParentId = block.Id,
            CellType = CellType.Text
        };
        block.Properties = properties;
        
        // Use scoped IBlocksDal service
        using(var scope = _serviceProvider.CreateScope())
        {
            // Store block using service
            var dal = scope.ServiceProvider.GetRequiredService<IBlocksDal>();
            await dal.Add(block);
        }
    }

}
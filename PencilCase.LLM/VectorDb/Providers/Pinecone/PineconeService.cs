using Microsoft.Extensions.Configuration;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Telemetry.LLM.RAG;
using PencilCase.Telemetry.Data.Database;
using Pinecone;

namespace PencilCase.LLM.VectorDb.Providers.Pinecone;

public class PineconeService : IRagService
{
    private readonly DAL<RagOperationEntry> _ragOpTelemetry;
    private readonly string _embedModel;
    private readonly string _defaultNamespace;
    private readonly PineconeClient _client;
    private readonly string _defaultIndex;

    private const string ParentIdMetadataKey = "parent_id"; 
    private const string TextMetadataKey = "text"; 
    private const char IdSeparator = ':';

    public PineconeService(IConfiguration configuration, 
        DAL<RagOperationEntry> ragOpTelemetry)
    {
        _ragOpTelemetry = ragOpTelemetry;
        var apiKey = configuration["Pinecone:ApiKey"];
        _client = new PineconeClient(apiKey);
        _embedModel = configuration["Pinecone:EmbedModel"] ?? "multilingual-e5-large";
        _defaultIndex = configuration["Pinecone:DefaultIndex"] ?? "pencilcase";
        _defaultNamespace = configuration["Pinecone:DefaultNamespace"] ?? "";
    }
    
    public async Task<List<RagDocument>?> GetChunksForQuery(string query, List<Guid> filterIds, uint nResults = 3)
    {
        var queryEmbedding = await EmbedDocuments(new List<RagDocument> { new(){ Content = query } });
        
        var queryVector = queryEmbedding.Data.Select((e) => 
            e.Values?.Select(Convert.ToSingle).ToArray()).FirstOrDefault();
        
        var index = _client.Index(_defaultIndex);
        
        var queryResults = await index.QueryAsync(new QueryRequest()
        {
            Vector = new ReadOnlyMemory<float>(queryVector),
            Namespace = _defaultNamespace,
            TopK = nResults,
            IncludeMetadata = true,
            IncludeValues = false,
            Filter = new Metadata()
            {
                [ParentIdMetadataKey] = new Metadata()
                {
                    ["$in"] = filterIds.Select(x => x.ToString()).ToArray()
                }
            }
        });

        var resultDocs = queryResults
            .Matches?
            .Select(MapMatchToDocument)
            .ToList();
        
        var telemetryEntry = PineconeTelemetryEntryHelpers
            .BuildQueryTelemetryEntry(queryResults, queryEmbedding, resultDocs);
        await TryAddEntryToTelemetry(telemetryEntry);
        
        return resultDocs;
    }

    public async Task AddChunks(List<RagDocument> docs)
    {
        var embeddings = await EmbedDocuments(docs);

        var embeddingsData = embeddings.Data;

        var index = _client.Index(_defaultIndex);

        List<Vector> records = embeddingsData.Select((e, idx) => new Vector()
        {
            Id = GetDocumentId(docs[idx]),
            Values = new ReadOnlyMemory<float>(e.Values?.Select(Convert.ToSingle).ToArray()),
            Metadata = new Metadata()
            {
                [ParentIdMetadataKey] = docs[idx].ParentId.ToString(),
                [TextMetadataKey] = docs[idx].Content,
            }
        }).ToList();

        await index.UpsertAsync(new UpsertRequest()
        {
            Vectors = records,
            Namespace = _defaultNamespace
        });
        
        var telemetryEntry = PineconeTelemetryEntryHelpers
            .BuildWriteTelemetryEntry(embeddings, docs);
        await TryAddEntryToTelemetry(telemetryEntry);
    }

    public async Task DeleteChunks(List<RagDocument> chunks)
    {
        foreach (var chunk in chunks)
        {
            await DeleteSingleDoc(chunk);
        }
    }

    public async Task DeleteSingleDoc(RagDocument doc)
    {
        try
        {
            var index = _client.Index(_defaultIndex);

            await index.DeleteAsync(new DeleteRequest {
                Ids = new List<string> { GetDocumentId(doc) },
                Namespace = _defaultNamespace,
            });
        }
        catch (PineconeApiException ex)
        {
            if(ex.Message.Contains("Error with gRPC status code 5"))
                throw new ArgumentException(ex.Message);
            else
                throw;
        }
    }

    private async Task<EmbeddingsList> EmbedDocuments(List<RagDocument> docs)
    {
        return await _client.Inference.EmbedAsync(new EmbedRequest()
        {
            Model = _embedModel,
            Inputs = docs.Select(i => new EmbedRequestInputsItem() { Text = i.Content }),
            Parameters = new EmbedRequestParameters()
            {
                InputType = "passage",
                Truncate = "END"
            }
        });
    }

    private RagDocument MapMatchToDocument(ScoredVector match)
    {
        var id = Guid.Parse(match.Id.Split(IdSeparator).LastOrDefault() ?? "");
        var parentId = Guid.Parse(match.Metadata?[ParentIdMetadataKey]?.Value.ToString() ?? string.Empty);
        var text = match.Metadata?[TextMetadataKey]?.Value.ToString() ?? string.Empty;
        return new RagDocument()
        {
            Id = id,
            ParentId = parentId,
            Content = text
        };
    }

    private String GetDocumentId(RagDocument doc)
    {
        return $"{doc.ParentId}{IdSeparator}{doc.Id}";
    }

    private async Task TryAddEntryToTelemetry(RagOperationEntry entry)
    {
        try
        {
            await _ragOpTelemetry.Add(entry);
        }
        catch (Exception)
        {
            return;
        }
    }
}
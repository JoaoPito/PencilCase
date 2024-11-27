using Microsoft.EntityFrameworkCore;
using PencilCase.Shared.Models.Telemetry.LLM.Agents;
using PencilCase.Shared.Models.Telemetry.LLM.RAG;

namespace PencilCase.Telemetry.Data.Database;

public class TelemetryDbContext : DbContext
{
    public virtual DbSet<GenerationResultEntry> GenerationResultEntries { get; set; }
    public virtual DbSet<RagOperationEntry> RagOperationEntries { get; set; }

    public TelemetryDbContext(DbContextOptions options) : base(options)
    {
        
    }
}
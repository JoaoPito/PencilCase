using Microsoft.EntityFrameworkCore;
using PencilCase.Shared.Models.Telemetry.LLM.Agents;

namespace PencilCase.Telemetry.Data.Database;

public class TelemetryDbContext : DbContext
{
    public virtual DbSet<GenerationResultEntry> GenerationResultEntries { get; set; }

    public TelemetryDbContext(DbContextOptions options) : base(options)
    {
        
    }
}
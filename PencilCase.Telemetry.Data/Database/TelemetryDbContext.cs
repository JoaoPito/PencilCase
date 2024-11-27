using Microsoft.EntityFrameworkCore;
using PencilCase.Shared.Models.Telemetry.LLM.Agents;

namespace PencilCase.Telemetry.Data.Database;

public class TelemetryDbContext : DbContext
{
    public List<GenerationResultEntry> GenerationResultEntries { get; set; } = new();

    public TelemetryDbContext(DbContextOptions options) : base(options)
    {
        
    }
}
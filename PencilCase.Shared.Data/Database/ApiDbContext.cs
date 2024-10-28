using Microsoft.EntityFrameworkCore;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Shared.Data.Database;

public class ApiDbContext : DbContext
{
    public List<Block> Blocks { get; set; } = new List<Block>();
    public List<BlockProperties> BlockProperties { get; set; } = new List<BlockProperties>();

    public ApiDbContext(DbContextOptions options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Block>()
            .HasMany<BlockProperties>();
        base.OnModelCreating(modelBuilder);
    }
}

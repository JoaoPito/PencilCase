using Microsoft.EntityFrameworkCore;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Shared.Data.Database;

public class BlocksDbContext : DbContext
{
    public List<Block> Blocks { get; set; } = new List<Block>();
    public List<BlockProperties> BlockProperties { get; set; } = new List<BlockProperties>();

    public BlocksDbContext(DbContextOptions options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Block>()
            .HasMany(b => b.Children)
            .WithOne(b => b.Parent)
            .HasForeignKey(b => b.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BlockProperties>()
            .HasKey(p => p.ParentId);

        modelBuilder.Entity<Block>()
            .HasOne(b => b.Properties)
            .WithOne(p => p.Parent)
            .HasForeignKey<BlockProperties>(p => p.ParentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);;
        base.OnModelCreating(modelBuilder);
    }
}

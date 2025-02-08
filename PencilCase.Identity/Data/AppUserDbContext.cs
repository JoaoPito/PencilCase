using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PencilCase.Identity.Models;

namespace PencilCase.Identity.Data;

public class AppUserDbContext(DbContextOptions<AppUserDbContext> opts): IdentityDbContext<AppUser>(opts)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AppUser>()
            .Property(u => u.Email)
            .HasMaxLength(320)
            .IsRequired();
        
        builder.Entity<AppUser>()
            .Property(u => u.UserName)
            .HasMaxLength(256)
            .IsRequired();
        
        base.OnModelCreating(builder);
    }
}
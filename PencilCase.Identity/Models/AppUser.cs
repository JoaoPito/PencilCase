using Microsoft.AspNetCore.Identity;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Identity.Models;

public class AppUser: IdentityUser
{
    public Guid RootBlockId { get; set; }
    public virtual Block RootBlock { get; set; } = new();
}
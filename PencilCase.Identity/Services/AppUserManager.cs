using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PencilCase.Identity.Models;
using PencilCase.Shared.Data.Database;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Identity.Services;

public class AppUserManager(
    IUserStore<AppUser> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<AppUser> passwordHasher,
    IEnumerable<IUserValidator<AppUser>> userValidators,
    IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<UserManager<AppUser>> logger)
    : UserManager<AppUser>(
        store, 
        optionsAccessor, 
        passwordHasher, 
        userValidators, 
        passwordValidators, 
        keyNormalizer,
        errors,
        services, 
        logger)
{
    private readonly IServiceProvider _servicesProvider = services;

    public override async Task<IdentityResult> CreateAsync(AppUser user, string password)
    {
        await CreateAndAddRootBlockToUser(user);
        return await base.CreateAsync(user, password);
    }

    private async Task CreateAndAddRootBlockToUser(AppUser user)
    {
        using var scope = _servicesProvider.CreateScope();
        var blocksDal = scope.ServiceProvider.GetRequiredService<IBlocksDal>();
        var rootBlock = CreateNewUserRootBlock(user);
        await blocksDal.Add(rootBlock);
        user.RootBlockId = rootBlock.Id;
    }

    private Block CreateNewUserRootBlock(AppUser user)
    {
        var block = new Block()
        {
            Id = Guid.NewGuid()
        };
        
        var properties = new BlockProperties()
        {
            Parent = block,
            ParentId = block.Id
        };

        block.OwnerId = Guid.Parse(user.Id);
        block.Name = $"{user.UserName}'s Workspace";
        block.Type = BlockType.Topic;
        block.Properties = properties;
        
        return block;
    }
}
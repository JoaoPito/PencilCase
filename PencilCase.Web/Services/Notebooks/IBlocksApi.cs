using PencilCase.Shared.DTOs.Requests.Blocks;
using PencilCase.Shared.DTOs.Responses.Blocks;
using PencilCase.Web.Pages.Notebooks.Models;

namespace PencilCase.Web.Services;

public interface IBlocksApi
{
    public Task<BlockViewModel?> GetBlock(Guid id);
    public Task<List<BlockViewModel>> GetChildren(Guid parentId);
    public Task<BlockViewModel?> AddBlock(BlockViewModel block);
    public Task<BlockViewModel?> GetLastUsedBlock();
    public Task<IEnumerable<BlockViewModel>> LoadAllAsync(IEnumerable<Guid> blockIds);
    public Task UpdateBlock(BlockViewModel block);
    public Task DeleteBlock(Guid id);
}
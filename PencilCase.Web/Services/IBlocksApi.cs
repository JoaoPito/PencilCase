using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Web.Services;

public interface IBlocksApi
{
    public Task<Block?> GetBlock(string id);
    public void AddBlock(Block block);
    public Task<Block?> GetLastUsedBlock();
}
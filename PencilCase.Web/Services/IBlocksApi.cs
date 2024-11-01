using PencilCase.Shared.DTOs.Requests.Blocks;
using PencilCase.Shared.DTOs.Responses.Blocks;

namespace PencilCase.Web.Services;

public interface IBlocksApi
{
    public Task<BlockResponse?> GetBlock(Guid id);
    public Task AddBlock(BlockPostRequest block);
    public Task<BlockResponse?> GetLastUsedBlock();
}
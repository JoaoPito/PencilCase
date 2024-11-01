using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Web.Services;

public interface IBlocksApi
{
    public Block GetBlock(string id);
    public Block GetLastUsedBlock();
}
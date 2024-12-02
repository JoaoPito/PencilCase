using Microsoft.EntityFrameworkCore;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Shared.Data.Database;

public interface IBlocksDal : IDal<Block>
{
    public List<Guid> GetIdsFromSubtreeWithType(Guid rootId, Func<Block, bool> criteria);
}
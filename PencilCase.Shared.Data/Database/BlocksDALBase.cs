using Microsoft.EntityFrameworkCore;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Shared.Data.Database;

public abstract class BlocksDALBase(DbContext context) : DAL<Block>(context)
{
    public abstract List<Guid> GetIdsFromSubtreeWithType(Guid rootId, Func<Block, bool> criteria);
}
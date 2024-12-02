using Microsoft.EntityFrameworkCore;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Shared.Data.Database;

public class BlocksDAL(BlocksDbContext context) : DAL<Block>(context), IBlocksDal
{
    private const string TableName = "Block";
    private const string SubtreeSqlQuery = @"
            WITH RECURSIVE descendants AS (
                SELECT ""Id"", ""ParentId"", ""Name"", ""Type""
                FROM """ + TableName + @"""
                WHERE ""Id"" = {0}
                UNION ALL
                SELECT b.""Id"", b.""ParentId"", b.""Name"", b.""Type""
                FROM """ + TableName + @""" b
                INNER JOIN descendants d ON b.""ParentId"" = d.""Id""
            )
            SELECT * FROM descendants";

    public List<Guid> GetIdsFromSubtreeWithType(Guid rootId, Func<Block, bool> criteria)
    {
        Console.WriteLine($"GetIdsFromSubtreeWithType {rootId}");
        
        return Context.Set<Block>()
            .FromSqlRaw(SubtreeSqlQuery, 
                rootId)
            .Where(criteria)
            .Select(b => b.Id)
            .ToList();
    }
}
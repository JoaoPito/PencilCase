using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Tests.Helpers;

public static class ExampleBlocksHelper
{
    public static List<Block> CreateRootWithSingleNotebook()
    {
        var blockList = new List<Block>();
        
        var root = new Block(){ Id = Guid.NewGuid(), Name = "root" };
        blockList.Add(root);
        blockList.Add(root.AddNotebook("mathematics"));
        return blockList;
    }
}
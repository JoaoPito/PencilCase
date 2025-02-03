using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Web.Client.ViewModels;

public class BlockViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BlockType Type { get; set; }
    public BlockPropertiesViewModel Properties { get; set; } = new BlockPropertiesViewModel();
    public IEnumerable<Guid> ChildrenIds { get; set; } = new List<Guid>();
    public Guid? ParentId { get; set; }
}

public class BlockPropertiesViewModel
{
    public int Order { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime LastModified { get; set; }
    public CellType CellType { get; set; }
    public Guid? CellShownAnswerId { get; set; }
}
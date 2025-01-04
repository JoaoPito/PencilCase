using System;

namespace PencilCase.Shared.Models.Notebooks;

public class BlockProperties
{
    public virtual Block Parent { get; set; } = null!;
    public Guid ParentId { get; set; } = new Guid();
    public int Order { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public CellType CellType { get; set; } = CellType.Text;
    public Guid CellShownAnswerId { get; set; }
}

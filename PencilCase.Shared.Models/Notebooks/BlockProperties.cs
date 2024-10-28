using System;

namespace PencilCase.Shared.Models.Notebooks;

public class BlockProperties
{
    public Guid Id { get; set; } = new Guid();
    public Block Parent { get; set; } = new Block();
    public Guid ParentId { get; set; } = new Guid();
    public int Order { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;

}

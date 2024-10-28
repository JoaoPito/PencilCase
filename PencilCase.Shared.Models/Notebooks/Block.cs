using System;

namespace PencilCase.Shared.Models.Notebooks;

public record Block
{
    public Guid Id { get; set; } = new Guid();
    public BlockType Type { get; set; } = BlockType.Topic;
    public string Name { get; set; } = String.Empty;
    public BlockProperties? Properties { get; set; }
    public virtual ICollection<Block> Children { get; set; } = new List<Block>();
    public virtual Block? Parent { get; set; } = null;
    public Guid? ParentId { get; set; }
}

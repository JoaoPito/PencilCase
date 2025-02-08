using System;

namespace PencilCase.Shared.Models.Notebooks;

public record Block
{
    public Guid Id { get; set; } = new Guid();
    public Guid OwnerId { get; set; } = Guid.Empty;
    public BlockType Type { get; set; } = BlockType.Topic;
    public string Name { get; set; } = String.Empty;
    public virtual BlockProperties? Properties { get; set; }
    public virtual ICollection<Block> Children { get; set; } = new List<Block>();
    public virtual Block? Parent { get; set; } = null;
    public Guid? ParentId { get; set; }

    public override string ToString() => Name;
    public virtual bool Equals(Block? other)
    {
        return other != null &&
               OwnerId == other.OwnerId &&
               Id == other.Id &&
               Name == other.Name &&
               ParentId == other.ParentId &&
               Type == other.Type;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, (int)Type, Name, Properties, Children, Parent, ParentId);
    }
}

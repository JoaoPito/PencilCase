using System;

namespace PencilCase.Shared.Models.Notebooks;

public class BlockProperties
{
    public Guid Id { get; set; } = new Guid();
    public int Order { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;

}

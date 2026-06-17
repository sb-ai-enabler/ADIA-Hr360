using System;
using System.Collections.Generic;

namespace Hr360.Domain;

public partial class ReviewTemplate
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Version { get; set; }

    public string DefinitionJson { get; set; } = null!;

    public bool IsArchived { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public virtual ICollection<ReviewCycle> ReviewCycles { get; set; } = new List<ReviewCycle>();
}

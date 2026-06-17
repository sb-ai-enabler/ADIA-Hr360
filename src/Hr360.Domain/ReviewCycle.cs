using System;
using System.Collections.Generic;

namespace Hr360.Domain;

public partial class ReviewCycle
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid TemplateId { get; set; }

    public int TemplateVersion { get; set; }

    public string TemplateSnapshotJson { get; set; } = null!;

    public int Status { get; set; }

    public DateOnly StartsOn { get; set; }

    public DateOnly EndsOn { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public virtual ICollection<ReviewAssignment> ReviewAssignments { get; set; } = new List<ReviewAssignment>();

    public virtual ReviewTemplate Template { get; set; } = null!;
}

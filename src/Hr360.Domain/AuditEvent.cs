using System;
using System.Collections.Generic;

namespace Hr360.Domain;

public partial class AuditEvent
{
    public Guid Id { get; set; }

    public string Actor { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public Guid EntityId { get; set; }

    public string MetadataJson { get; set; } = null!;

    public DateTimeOffset OccurredAt { get; set; }
}

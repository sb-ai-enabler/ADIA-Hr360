namespace Hr360.Domain;

public partial class AuditEvent
{
    public AuditEvent()
    {
        Id = Guid.NewGuid();
        MetadataJson = "{}";
        OccurredAt = DateTimeOffset.UtcNow;
    }
}

public partial class Employee
{
    public Employee()
    {
        Id = Guid.NewGuid();
        IsActive = true;
    }
}

public partial class FeedbackSubmission
{
    public FeedbackSubmission()
    {
        Id = Guid.NewGuid();
        AnswersJson = "[]";
        IdempotencyKey = string.Empty;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public partial class ReviewAssignment
{
    public ReviewAssignment()
    {
        Id = Guid.NewGuid();
        RowVersion = [];
        Status = (int)Hr360.Shared.AssignmentStatus.NotStarted;
    }
}

public partial class ReviewCycle
{
    public ReviewCycle()
    {
        Id = Guid.NewGuid();
        TemplateSnapshotJson = "{}";
        Status = (int)Hr360.Shared.ReviewCycleStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

public partial class ReviewTemplate
{
    public ReviewTemplate()
    {
        Id = Guid.NewGuid();
        Description = string.Empty;
        DefinitionJson = "{}";
        Version = 1;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

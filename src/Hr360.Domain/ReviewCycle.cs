using Hr360.Shared;

namespace Hr360.Domain;

public sealed class ReviewCycle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid TemplateId { get; set; }
    public int TemplateVersion { get; set; }
    public string TemplateSnapshotJson { get; set; } = "{}";
    public ReviewCycleStatus Status { get; set; } = ReviewCycleStatus.Draft;
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public List<ReviewAssignment> Assignments { get; set; } = [];
}

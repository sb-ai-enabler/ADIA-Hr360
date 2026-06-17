using Hr360.Shared;

namespace Hr360.Domain;

public sealed class ReviewAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CycleId { get; set; }
    public ReviewCycle? Cycle { get; set; }
    public Guid RevieweeId { get; set; }
    public Employee? Reviewee { get; set; }
    public Guid ReviewerId { get; set; }
    public Employee? Reviewer { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.NotStarted;
    public DateTimeOffset? SubmittedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

namespace Hr360.Domain;

public sealed class FeedbackSubmission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssignmentId { get; set; }
    public ReviewAssignment? Assignment { get; set; }
    public string AnswersJson { get; set; } = "[]";
    public bool IsFinal { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? ClientDraftId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SubmittedAt { get; set; }
}

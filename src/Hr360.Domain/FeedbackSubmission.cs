using System;
using System.Collections.Generic;

namespace Hr360.Domain;

public partial class FeedbackSubmission
{
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }

    public string AnswersJson { get; set; } = null!;

    public bool IsFinal { get; set; }

    public string IdempotencyKey { get; set; } = null!;

    public string? ClientDraftId { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }

    public virtual ReviewAssignment Assignment { get; set; } = null!;
}

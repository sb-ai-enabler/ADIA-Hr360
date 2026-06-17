using System;
using System.Collections.Generic;

namespace Hr360.Domain;

public partial class ReviewAssignment
{
    public Guid Id { get; set; }

    public Guid CycleId { get; set; }

    public Guid RevieweeId { get; set; }

    public Guid ReviewerId { get; set; }

    public int Status { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ReviewCycle Cycle { get; set; } = null!;

    public virtual ICollection<FeedbackSubmission> FeedbackSubmissions { get; set; } = new List<FeedbackSubmission>();

    public virtual Employee Reviewee { get; set; } = null!;

    public virtual Employee Reviewer { get; set; } = null!;
}

using Hr360.Domain;

namespace Hr360.Application.Interfaces;

public interface IFeedbackSubmissionRepository : IRepository<FeedbackSubmission>
{
    Task<FeedbackSubmission?> GetLatestDraftAsync(Guid assignmentId, CancellationToken cancellationToken = default);

    Task<bool> HasFinalSubmissionAsync(string idempotencyKey, CancellationToken cancellationToken = default);
}

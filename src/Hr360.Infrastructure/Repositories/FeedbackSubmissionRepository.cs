using Hr360.Application.Interfaces;
using Hr360.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hr360.Infrastructure.Repositories;

public sealed class FeedbackSubmissionRepository(Hr360DbContext context)
    : RepositoryBase<FeedbackSubmission>(context), IFeedbackSubmissionRepository
{
    public async Task<FeedbackSubmission?> GetLatestDraftAsync(Guid assignmentId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(s => s.AssignmentId == assignmentId && !s.IsFinal)
            .OrderByDescending(s => s.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> HasFinalSubmissionAsync(string idempotencyKey, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(s => s.IdempotencyKey == idempotencyKey && s.IsFinal, cancellationToken);
}

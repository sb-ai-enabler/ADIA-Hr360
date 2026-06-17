using Hr360.Application.Interfaces;
using Hr360.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hr360.Infrastructure.Repositories;

public sealed class ReviewAssignmentRepository(Hr360DbContext context)
    : RepositoryBase<ReviewAssignment>(context), IReviewAssignmentRepository
{
    public async Task<IReadOnlyList<ReviewAssignment>> GetForReviewerAsync(
        string reviewerObjectId,
        CancellationToken cancellationToken = default) =>
        await Set
            .Include(a => a.Cycle)
            .Include(a => a.Reviewee)
            .Include(a => a.Reviewer)
            .Where(a => a.Reviewer!.EntraObjectId == reviewerObjectId)
            .OrderBy(a => a.Cycle!.EndsOn)
            .ToListAsync(cancellationToken);

    public async Task<ReviewAssignment?> GetWithCycleAndReviewerAsync(
        Guid assignmentId,
        CancellationToken cancellationToken = default) =>
        await Set
            .Include(a => a.Cycle)
            .Include(a => a.Reviewer)
            .SingleOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);
}

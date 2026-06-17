using Hr360.Domain;

namespace Hr360.Application.Interfaces;

public interface IReviewAssignmentRepository : IRepository<ReviewAssignment>
{
    Task<IReadOnlyList<ReviewAssignment>> GetForReviewerAsync(
        string reviewerObjectId,
        CancellationToken cancellationToken = default);

    Task<ReviewAssignment?> GetWithCycleAndReviewerAsync(
        Guid assignmentId,
        CancellationToken cancellationToken = default);
}

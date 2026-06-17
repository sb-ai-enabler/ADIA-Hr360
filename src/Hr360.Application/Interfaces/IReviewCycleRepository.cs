using Hr360.Domain;

namespace Hr360.Application.Interfaces;

public interface IReviewCycleRepository : IRepository<ReviewCycle>
{
    Task<IReadOnlyList<ReviewCycle>> GetAllWithAssignmentsAsync(CancellationToken cancellationToken = default);
}

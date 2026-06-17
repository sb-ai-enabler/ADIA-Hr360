using Hr360.Application.Interfaces;
using Hr360.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hr360.Infrastructure.Repositories;

public sealed class ReviewCycleRepository(Hr360DbContext context)
    : RepositoryBase<ReviewCycle>(context), IReviewCycleRepository
{
    public async Task<IReadOnlyList<ReviewCycle>> GetAllWithAssignmentsAsync(CancellationToken cancellationToken = default) =>
        await Set
            .Include(c => c.Assignments)
            .OrderByDescending(c => c.StartsOn)
            .ToListAsync(cancellationToken);
}

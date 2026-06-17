using Hr360.Application.Interfaces;
using Hr360.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hr360.Infrastructure.Repositories;

public sealed class ReviewTemplateRepository(Hr360DbContext context)
    : RepositoryBase<ReviewTemplate>(context), IReviewTemplateRepository
{
    public async Task<IReadOnlyList<ReviewTemplate>> GetAllOrderedByCreatedDescAsync(CancellationToken cancellationToken = default) =>
        await Set
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<ReviewTemplate?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Set.SingleOrDefaultAsync(t => t.Id == id && !t.IsArchived, cancellationToken);
}

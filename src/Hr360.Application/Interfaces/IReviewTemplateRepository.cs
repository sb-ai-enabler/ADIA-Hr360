using Hr360.Domain;

namespace Hr360.Application.Interfaces;

public interface IReviewTemplateRepository : IRepository<ReviewTemplate>
{
    Task<IReadOnlyList<ReviewTemplate>> GetAllOrderedByCreatedDescAsync(CancellationToken cancellationToken = default);

    Task<ReviewTemplate?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

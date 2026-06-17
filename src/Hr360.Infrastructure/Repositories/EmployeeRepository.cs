using Hr360.Application.Interfaces;
using Hr360.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hr360.Infrastructure.Repositories;

public sealed class EmployeeRepository(Hr360DbContext context)
    : RepositoryBase<Employee>(context), IEmployeeRepository
{
    public async Task<IReadOnlyList<Employee>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await Set
            .Where(e => e.IsActive)
            .OrderBy(e => e.DisplayName)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetActiveIdsAsync(
        IReadOnlyCollection<Guid> candidateIds,
        CancellationToken cancellationToken = default) =>
        await Set
            .Where(e => e.IsActive && candidateIds.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);
}

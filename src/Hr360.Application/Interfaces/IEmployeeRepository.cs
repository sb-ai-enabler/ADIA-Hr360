using Hr360.Domain;

namespace Hr360.Application.Interfaces;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<IReadOnlyList<Employee>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetActiveIdsAsync(
        IReadOnlyCollection<Guid> candidateIds,
        CancellationToken cancellationToken = default);
}

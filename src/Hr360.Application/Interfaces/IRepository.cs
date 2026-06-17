namespace Hr360.Application.Interfaces;

/// <summary>
/// Common persistence operations shared by all entity repositories.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);
    void Add(T entity);
    void Remove(T entity);
}

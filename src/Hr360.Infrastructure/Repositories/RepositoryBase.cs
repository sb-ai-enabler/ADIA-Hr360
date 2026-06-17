using Hr360.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hr360.Infrastructure.Repositories;

public abstract class RepositoryBase<T>(Hr360DbContext context) : IRepository<T>
    where T : class
{
    protected Hr360DbContext Context { get; } = context;

    protected DbSet<T> Set => Context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Set.FindAsync([id], cancellationToken);

    public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default) =>
        await Set.ToListAsync(cancellationToken);

    public void Add(T entity) => Set.Add(entity);

    public void Remove(T entity) => Set.Remove(entity);
}

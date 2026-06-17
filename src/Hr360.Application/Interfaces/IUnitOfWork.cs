namespace Hr360.Application.Interfaces;

/// <summary>
/// Coordinates persistence of changes made across repositories that share a context.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

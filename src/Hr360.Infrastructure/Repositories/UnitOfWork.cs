using Hr360.Application.Interfaces;

namespace Hr360.Infrastructure.Repositories;

public sealed class UnitOfWork(Hr360DbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}

using Hr360.Application.Interfaces;
using Hr360.Domain;

namespace Hr360.Infrastructure.Repositories;

public sealed class AuditEventRepository(Hr360DbContext context)
    : RepositoryBase<AuditEvent>(context), IAuditEventRepository;

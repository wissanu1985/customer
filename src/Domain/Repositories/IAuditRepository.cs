using Domain.Entities;
using Domain.Common;
using System.Linq.Expressions;

namespace Domain.Repositories;

public interface IAuditRepository : IRepository<Audit>
{
    Task<IReadOnlyList<Audit>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Audit>> GetByEntityIdAsync(string entityId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Audit>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Audit>> GetByActionAsync(string action, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Audit>> GetByChangedByAsync(string changedBy, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Audit>> GetRecentAuditsAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Audit>> GetByAuditTypeAsync(string auditType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Audit>> GetByTableNameAsync(string tableName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Audit>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<Audit, bool>>? predicate, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
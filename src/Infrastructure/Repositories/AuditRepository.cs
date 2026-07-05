using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

public sealed class AuditRepository : BaseRepository<Audit>, IAuditRepository
{
    public AuditRepository(DbContext context) : base(context) { }

    public async Task<IReadOnlyList<Audit>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(x => x.EntityType == entityType)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Audit>> GetByEntityIdAsync(string entityId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(x => x.EntityId == entityId)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Audit>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(x => x.Timestamp >= startDate && x.Timestamp <= endDate)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Audit>> GetByActionAsync(string action, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(x => x.Action == action)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Audit>> GetByChangedByAsync(string changedBy, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(x => x.ChangedBy == changedBy)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Audit>> GetRecentAuditsAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .OrderByDescending(x => x.Timestamp)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Audit>> GetByAuditTypeAsync(string auditType, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(x => x.AuditType == auditType)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Audit>> GetByTableNameAsync(string tableName, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(x => x.TableName == tableName)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Audit>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<Audit, bool>>? predicate, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken);
    }
}
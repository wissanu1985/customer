using Domain.Common;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Infrastructure.Repositories;

namespace Infrastructure.contexts;

public class UnitOfWork : IUnitOfWork
{
    private readonly CustomerContext _context;
    private IAuditRepository? _audits;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(CustomerContext context)
    {
        _context = context;
    }

    public IAuditRepository Audits => _audits ??= new AuditRepository(_context);

    public IRepository<T> Repository<T>() where T : AuditableEntity
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(BaseRepository<>).MakeGenericType(type);
            var repository = Activator.CreateInstance(repositoryType, _context);
            _repositories.Add(type, repository!);
        }
        return (IRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-assign Guid for newly added AuditableEntities with empty Id (DB has no default)
        foreach (var entry in _context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.Id == Guid.Empty)
                entry.Entity.Id = Guid.NewGuid();
        }

        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(Func<IUnitOfWork, Task> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await operation(this);
            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<IUnitOfWork, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation(this);
            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public void Dispose()
    {
        // DI container handles DbContext disposal; UnitOfWork no longer owns the context.
    }
}
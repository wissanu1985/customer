using Domain.Repositories;

namespace Domain.Common;

public interface IUnitOfWork : IDisposable
{
    IAuditRepository Audits { get; }
    IRepository<T> Repository<T>() where T : AuditableEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<IUnitOfWork, Task> operation, CancellationToken cancellationToken = default);
    Task<T> ExecuteInTransactionAsync<T>(Func<IUnitOfWork, Task<T>> operation, CancellationToken cancellationToken = default);
}
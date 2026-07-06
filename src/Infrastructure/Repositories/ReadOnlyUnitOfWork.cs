using Domain.Common;
using Infrastructure.contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ReadOnlyUnitOfWork : IReadOnlyUnitOfWork
{
    private readonly IDbContextFactory<CustomerContext> _factory;
    private readonly List<CustomerContext> _contexts = new();
    private readonly Dictionary<Type, object> _repositories = new();
    private bool _disposed;

    public ReadOnlyUnitOfWork(IDbContextFactory<CustomerContext> factory)
    {
        _factory = factory;
    }

    public IReadOnlyRepository<T> Repository<T>() where T : class
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var type = typeof(T);
        if (!_repositories.TryGetValue(type, out var repo))
        {
            var context = _factory.CreateDbContext();
            _contexts.Add(context);
            repo = new ReadOnlyRepository<T>(context);
            _repositories[type] = repo;
        }
        return (IReadOnlyRepository<T>)repo;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var ctx in _contexts)
            ctx.Dispose();

        _contexts.Clear();
        _repositories.Clear();
    }
}

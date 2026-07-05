namespace Domain.Common;

public interface IReadOnlyUnitOfWork : IDisposable
{
    IReadOnlyRepository<T> Repository<T>() where T : class;
}

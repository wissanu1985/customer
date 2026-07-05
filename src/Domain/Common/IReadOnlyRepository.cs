namespace Domain.Common;

public interface IReadOnlyRepository<T> where T : class
{
    IQueryable<T> Query();
}

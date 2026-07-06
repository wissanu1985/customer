using Domain.Common;
using Infrastructure.contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ReadOnlyRepository<T> : IReadOnlyRepository<T> where T : class
{
    private readonly CustomerContext _context;

    public ReadOnlyRepository(CustomerContext context)
    {
        _context = context;
    }

    public IQueryable<T> Query() => _context.Set<T>().AsNoTracking();
}

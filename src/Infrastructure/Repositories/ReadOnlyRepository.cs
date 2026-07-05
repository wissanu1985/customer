using Domain.Common;
using Infrastructure.contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ReadOnlyRepository<T> : IReadOnlyRepository<T> where T : class
{
    private readonly CitizenContext _context;

    public ReadOnlyRepository(CitizenContext context)
    {
        _context = context;
    }

    public IQueryable<T> Query() => _context.Set<T>().AsNoTracking();
}

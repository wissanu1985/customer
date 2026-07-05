using Application.Commons.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace Application.Commons.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int size,
        CancellationToken cancellationToken = default)
    {
        var total = await query.CountAsync(cancellationToken);

        var data = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Success(data, page, size, total);
    }
}
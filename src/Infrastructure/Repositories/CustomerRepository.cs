using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(CustomerContext context) : base(context) { }

    public Task<bool> ExistsByNationalIdAsync(string nationalId, Guid? excludingId, CancellationToken cancellationToken = default)
    {
        return Entities
            .Where(c => c.NationalId == nationalId)
            .Where(c => excludingId == null || c.Id != excludingId)
            .AnyAsync(cancellationToken);
    }

    public Task<Customer?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken = default)
    {
        return Entities.FirstOrDefaultAsync(c => c.NationalId == nationalId, cancellationToken);
    }
}

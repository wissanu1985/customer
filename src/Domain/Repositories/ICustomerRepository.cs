using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<bool> ExistsByNationalIdAsync(string nationalId, Guid? excludingId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken = default);
}

using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface ICitizenRepository : IRepository<Citizen>
{
    Task<bool> ExistsByIdCardAsync(string idCardNumber, Guid? excludingId, CancellationToken cancellationToken = default);
    Task<Citizen?> GetByIdCardAsync(string idCardNumber, CancellationToken cancellationToken = default);
}

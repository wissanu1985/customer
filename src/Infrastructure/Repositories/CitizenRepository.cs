using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CitizenRepository : BaseRepository<Citizen>, ICitizenRepository
{
    public CitizenRepository(CitizenContext context) : base(context) { }

    public Task<bool> ExistsByIdCardAsync(string idCardNumber, Guid? excludingId, CancellationToken cancellationToken = default)
    {
        return Entities
            .Where(c => c.IdCardNumber == idCardNumber)
            .Where(c => excludingId == null || c.Id != excludingId)
            .AnyAsync(cancellationToken);
    }

    public Task<Citizen?> GetByIdCardAsync(string idCardNumber, CancellationToken cancellationToken = default)
    {
        return Entities.FirstOrDefaultAsync(c => c.IdCardNumber == idCardNumber, cancellationToken);
    }
}

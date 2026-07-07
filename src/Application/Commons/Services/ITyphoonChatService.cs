using Application.Features.IdCardExtractions.Queries.ExtractIdCard;

namespace Application.Commons.Services;

public interface ITyphoonChatService
{
    Task<IdCardData> ExtractIdCardAsync(string markdown, CancellationToken cancellationToken = default);
}

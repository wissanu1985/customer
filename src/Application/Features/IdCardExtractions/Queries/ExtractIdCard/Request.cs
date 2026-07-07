using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.IdCardExtractions.Queries.ExtractIdCard;

public sealed record Request(byte[] ImageBytes, string FileName) : IRequest<Result<Response>>;

public sealed class Response
{
    public IdCardData Data { get; set; } = new();
}

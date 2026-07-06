using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Citizens.Queries.SearchCitizens;

public sealed record Request(
    string? Keyword,
    int Page = 1,
    int Size = 10) : IRequest<Result<IPagedResult<CitizenSearchItem>>>;

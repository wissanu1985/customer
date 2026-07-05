using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Citizens.Queries.GetCitizen;

public sealed record Request(Guid Id) : IRequest<Result<CitizenDetail>>;

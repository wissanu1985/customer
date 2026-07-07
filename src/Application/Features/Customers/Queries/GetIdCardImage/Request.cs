using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Customers.Queries.GetIdCardImage;

public sealed record Request(Guid Id) : IRequest<Result<string>>;

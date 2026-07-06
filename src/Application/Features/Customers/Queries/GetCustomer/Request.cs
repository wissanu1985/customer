using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Customers.Queries.GetCustomer;

public sealed record Request(Guid Id) : IRequest<Result<CustomerDetail>>;

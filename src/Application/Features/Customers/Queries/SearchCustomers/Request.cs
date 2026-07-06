using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Customers.Queries.SearchCustomers;

public sealed record Request(
    string? NationalId,
    string? FirstName,
    string? LastName,
    string? Province,
    string? District,
    string? SubDistrict,
    string? PostalCode,
    int Page = 1,
    int Size = 10) : IRequest<Result<IPagedResult<CustomerSearchItem>>>;

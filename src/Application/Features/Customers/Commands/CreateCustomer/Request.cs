using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Customers.Commands.CreateCustomer;

public sealed record Request(
    string NationalId,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string AddressLine1,
    string SubDistrict,
    string District,
    string Province,
    string PostalCode,
    string? IdCardImage = null) : IRequest<Result<Response>>;

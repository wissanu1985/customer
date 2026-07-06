using Application.Commons.Wrappers;
using Mediator;
using System.Text.Json.Serialization;

namespace Application.Features.Customers.Commands.UpdateCustomer;

public sealed record Request(
    [property: JsonIgnore] Guid Id,
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

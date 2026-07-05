using Application.Commons.Wrappers;
using Mediator;
using System.Text.Json.Serialization;

namespace Application.Features.Citizens.Commands.UpdateCitizen;

public sealed record Request(
    [property: JsonIgnore] Guid Id,
    string IdCardNumber,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string AddressLine1,
    string SubDistrict,
    string District,
    string Province,
    string PostalCode,
    string? IdCardImage = null) : IRequest<Result<Response>>;

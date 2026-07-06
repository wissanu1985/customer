using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Citizens.Commands.CreateCitizen;

public sealed record Request(
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

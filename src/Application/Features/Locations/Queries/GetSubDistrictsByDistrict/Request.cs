using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Locations.Queries.GetSubDistrictsByDistrict;

public sealed record Request(int DistrictID) : IRequest<Result<List<Response>>>;

public sealed class Response
{
    public int TambonID { get; set; }
    public int DistrictID { get; set; }
    public string TambonThai { get; set; } = string.Empty;
    public string TambonEng { get; set; } = string.Empty;
    public string TambonThaiShort { get; set; } = string.Empty;
    public string TambonEngShort { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

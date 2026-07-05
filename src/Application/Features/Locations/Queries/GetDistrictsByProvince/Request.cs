using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Locations.Queries.GetDistrictsByProvince;

public sealed record Request(int ProvinceID) : IRequest<Result<List<Response>>>;

public sealed class Response
{
    public int DistrictID { get; set; }
    public int ProvinceID { get; set; }
    public string DistrictThai { get; set; } = string.Empty;
    public string DistrictEng { get; set; } = string.Empty;
    public string DistrictThaiShort { get; set; } = string.Empty;
    public string DistrictEngShort { get; set; } = string.Empty;
}

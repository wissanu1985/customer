using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Locations.Queries.GetAllProvinces;

public sealed record Request : IRequest<Result<List<Response>>>;

public sealed class Response
{
    public int ProvinceID { get; set; }
    public string ProvinceThai { get; set; } = string.Empty;
    public string ProvinceEng { get; set; } = string.Empty;
}

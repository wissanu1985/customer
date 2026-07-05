using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Locations.Queries.GetDistrictsByProvince;

public sealed class RequestHandler : IRequestHandler<Request, Result<List<Response>>>
{
    private readonly IReadOnlyUnitOfWork _unitOfWork;

    public RequestHandler(IReadOnlyUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<List<Response>>> Handle(Request request, CancellationToken cancellationToken)
    {
        var districts = await _unitOfWork.Repository<District>().Query()
            .Where(d => d.ProvinceID == request.ProvinceID)
            .OrderBy(d => d.DistrictID)
            .Select(d => new Response
            {
                DistrictID = d.DistrictID,
                ProvinceID = d.ProvinceID,
                DistrictThai = d.DistrictThai,
                DistrictEng = d.DistrictEng,
                DistrictThaiShort = d.DistrictThaiShort,
                DistrictEngShort = d.DistrictEngShort
            })
            .ToListAsync(cancellationToken);

        return Result<List<Response>>.Success(districts);
    }
}

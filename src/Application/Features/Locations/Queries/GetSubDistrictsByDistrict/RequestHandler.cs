using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Locations.Queries.GetSubDistrictsByDistrict;

public sealed class RequestHandler : IRequestHandler<Request, Result<List<Response>>>
{
    private readonly IReadOnlyUnitOfWork _unitOfWork;

    public RequestHandler(IReadOnlyUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<List<Response>>> Handle(Request request, CancellationToken cancellationToken)
    {
        var subDistricts = await _unitOfWork.Repository<SubDistrict>().Query()
            .Where(s => s.DistrictID == request.DistrictID)
            .OrderBy(s => s.TambonID)
            .Select(s => new Response
            {
                TambonID = s.TambonID,
                DistrictID = s.DistrictID,
                TambonThai = s.TambonThai,
                TambonEng = s.TambonEng,
                TambonThaiShort = s.TambonThaiShort,
                TambonEngShort = s.TambonEngShort,
                PostalCode = s.PostalCode
            })
            .ToListAsync(cancellationToken);

        return Result<List<Response>>.Success(subDistricts);
    }
}

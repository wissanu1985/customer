using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Locations.Queries.GetAllProvinces;

public sealed class RequestHandler : IRequestHandler<Request, Result<List<Response>>>
{
    private readonly IReadOnlyUnitOfWork _unitOfWork;

    public RequestHandler(IReadOnlyUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<List<Response>>> Handle(Request request, CancellationToken cancellationToken)
    {
        var provinces = await _unitOfWork.Repository<Province>().Query()
            .OrderBy(p => p.ProvinceID)
            .Select(p => new Response
            {
                ProvinceID = p.ProvinceID,
                ProvinceThai = p.ProvinceThai,
                ProvinceEng = p.ProvinceEng
            })
            .ToListAsync(cancellationToken);

        return Result<List<Response>>.Success(provinces);
    }
}

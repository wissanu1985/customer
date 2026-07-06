using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Application.Features.Citizens.Queries.GetCitizen;

public sealed class RequestHandler : IRequestHandler<Request, Result<CitizenDetail>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<CitizenDetail>> Handle(Request request, CancellationToken cancellationToken)
    {
        var citizen = await _unitOfWork.Repository<Citizen>().Entities
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (citizen is null)
            return Result<CitizenDetail>.Failure("ไม่พบข้อมูลลูกค้า", HttpStatusCode.NotFound);

        return Result<CitizenDetail>.Success(new CitizenDetail
        {
            Id = citizen.Id,
            IdCardNumber = citizen.IdCardNumber,
            FirstName = citizen.FirstName,
            LastName = citizen.LastName,
            BirthDate = citizen.BirthDate,
            AddressLine1 = citizen.AddressLine1,
            SubDistrict = citizen.SubDistrict,
            District = citizen.District,
            Province = citizen.Province,
            PostalCode = citizen.PostalCode,
            IdCardImage = citizen.IdCardImage
        });
    }
}

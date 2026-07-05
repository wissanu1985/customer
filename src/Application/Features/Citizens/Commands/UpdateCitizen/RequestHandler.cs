using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Application.Features.Citizens.Commands.UpdateCitizen;

public sealed class RequestHandler : IRequestHandler<Request, Result<Response>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICitizenRepository _citizenRepository;

    public RequestHandler(IUnitOfWork unitOfWork, ICitizenRepository citizenRepository)
    {
        _unitOfWork = unitOfWork;
        _citizenRepository = citizenRepository;
    }

    public async ValueTask<Result<Response>> Handle(Request request, CancellationToken cancellationToken)
    {
        try
        {
            var citizen = await _unitOfWork.Repository<Citizen>().Entities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (citizen is null)
                return Result<Response>.Failure("ไม่พบข้อมูลลูกค้า", HttpStatusCode.NotFound);

            var duplicate = await _citizenRepository.ExistsByIdCardAsync(request.IdCardNumber, excludingId: request.Id, cancellationToken);
            if (duplicate)
                return Result<Response>.Failure("เลขบัตรประชาชนนี้มีอยู่แล้วในระบบ", HttpStatusCode.BadRequest);

            citizen.IdCardNumber = request.IdCardNumber;
            citizen.FirstName = request.FirstName;
            citizen.LastName = request.LastName;
            citizen.BirthDate = request.BirthDate;
            citizen.AddressLine1 = request.AddressLine1;
            citizen.SubDistrict = request.SubDistrict;
            citizen.District = request.District;
            citizen.Province = request.Province;
            citizen.PostalCode = request.PostalCode;
            citizen.IdCardImage = request.IdCardImage;
            citizen.UpdatedDate = DateTime.UtcNow;
            citizen.UpdatedBy = "System";

            _unitOfWork.Repository<Citizen>().Update(citizen);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Response>.Success(new Response(citizen.Id));
        }
        catch (Exception ex)
        {
            return Result<Response>.Failure($"Failed to update citizen: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}

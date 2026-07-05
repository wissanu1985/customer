using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Mediator;
using System.Net;

namespace Application.Features.Citizens.Commands.CreateCitizen;

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
            var exists = await _citizenRepository.ExistsByIdCardAsync(request.IdCardNumber, excludingId: null, cancellationToken);
            if (exists)
                return Result<Response>.Failure("เลขบัตรประชาชนนี้มีอยู่แล้วในระบบ", HttpStatusCode.BadRequest);

            var entity = new Citizen()
            {
                IdCardNumber = request.IdCardNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                AddressLine1 = request.AddressLine1,
                SubDistrict = request.SubDistrict,
                District = request.District,
                Province = request.Province,
                PostalCode = request.PostalCode,
                IdCardImage = request.IdCardImage
            };

            await _unitOfWork.Repository<Citizen>().AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Response>.Success(new Response(entity.Id), statusCode: HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return Result<Response>.Failure($"Failed to create citizen: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}

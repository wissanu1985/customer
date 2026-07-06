using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Mediator;
using System.Net;

namespace Application.Features.Customers.Commands.CreateCustomer;

public sealed class RequestHandler : IRequestHandler<Request, Result<Response>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerRepository _customerRepository;

    public RequestHandler(IUnitOfWork unitOfWork, ICustomerRepository customerRepository)
    {
        _unitOfWork = unitOfWork;
        _customerRepository = customerRepository;
    }

    public async ValueTask<Result<Response>> Handle(Request request, CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _customerRepository.ExistsByNationalIdAsync(request.NationalId, excludingId: null, cancellationToken);
            if (exists)
                return Result<Response>.Failure("เลขบัตรประจำตัวนี้มีอยู่แล้วในระบบ", HttpStatusCode.BadRequest);

            var entity = new Customer()
            {
                NationalId = request.NationalId,
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

            await _unitOfWork.Repository<Customer>().AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Response>.Success(new Response(entity.Id), statusCode: HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return Result<Response>.Failure($"Failed to create customer: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}

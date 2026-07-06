using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Application.Features.Customers.Commands.UpdateCustomer;

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
            var customer = await _unitOfWork.Repository<Customer>().Entities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (customer is null)
                return Result<Response>.Failure("ไม่พบข้อมูลลูกค้า", HttpStatusCode.NotFound);

            var duplicate = await _customerRepository.ExistsByNationalIdAsync(request.NationalId, excludingId: request.Id, cancellationToken);
            if (duplicate)
                return Result<Response>.Failure("เลขบัตรประจำตัวนี้มีอยู่แล้วในระบบ", HttpStatusCode.BadRequest);

            customer.NationalId = request.NationalId;
            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.BirthDate = request.BirthDate;
            customer.AddressLine1 = request.AddressLine1;
            customer.SubDistrict = request.SubDistrict;
            customer.District = request.District;
            customer.Province = request.Province;
            customer.PostalCode = request.PostalCode;
            customer.IdCardImage = request.IdCardImage;
            customer.UpdatedDate = DateTime.UtcNow;
            customer.UpdatedBy = "System";

            _unitOfWork.Repository<Customer>().Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Response>.Success(new Response(customer.Id));
        }
        catch (Exception ex)
        {
            return Result<Response>.Failure($"Failed to update customer: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}

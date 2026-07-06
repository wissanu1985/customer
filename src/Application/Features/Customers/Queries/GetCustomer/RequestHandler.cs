using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Application.Features.Customers.Queries.GetCustomer;

public sealed class RequestHandler : IRequestHandler<Request, Result<CustomerDetail>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<CustomerDetail>> Handle(Request request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Repository<Customer>().Entities
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer is null)
            return Result<CustomerDetail>.Failure("ไม่พบข้อมูลลูกค้า", HttpStatusCode.NotFound);

        return Result<CustomerDetail>.Success(new CustomerDetail
        {
            Id = customer.Id,
            NationalId = customer.NationalId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            BirthDate = customer.BirthDate,
            AddressLine1 = customer.AddressLine1,
            SubDistrict = customer.SubDistrict,
            District = customer.District,
            Province = customer.Province,
            PostalCode = customer.PostalCode,
            IdCardImage = customer.IdCardImage
        });
    }
}

using Application.Commons.Extensions;
using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Queries.SearchCustomers;

public sealed class RequestHandler : IRequestHandler<Request, Result<IPagedResult<CustomerSearchItem>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<IPagedResult<CustomerSearchItem>>> Handle(Request request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Customer>().Entities.AsQueryable();

        var nationalId = request.NationalId?.Trim();
        if (!string.IsNullOrWhiteSpace(nationalId))
            query = query.Where(c => c.NationalId.Contains(nationalId));

        var firstName = request.FirstName?.Trim();
        if (!string.IsNullOrWhiteSpace(firstName))
            query = query.Where(c => c.FirstName.Contains(firstName));

        var lastName = request.LastName?.Trim();
        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(c => c.LastName.Contains(lastName));

        var province = request.Province?.Trim();
        if (!string.IsNullOrWhiteSpace(province))
            query = query.Where(c => c.Province == province);

        var district = request.District?.Trim();
        if (!string.IsNullOrWhiteSpace(district))
            query = query.Where(c => c.District == district);

        var subDistrict = request.SubDistrict?.Trim();
        if (!string.IsNullOrWhiteSpace(subDistrict))
            query = query.Where(c => c.SubDistrict == subDistrict);

        var postalCode = request.PostalCode?.Trim();
        if (!string.IsNullOrWhiteSpace(postalCode))
            query = query.Where(c => c.PostalCode == postalCode);

        var paged = await query
            .OrderByDescending(c => c.CreatedDate)
            .ToPagedResultAsync(request.Page, request.Size, cancellationToken);

        var items = paged.Data
            .Select(c => new CustomerSearchItem
            {
                Id = c.Id,
                NationalId = c.NationalId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                BirthDate = c.BirthDate,
                AddressLine1 = c.AddressLine1,
                SubDistrict = c.SubDistrict,
                District = c.District,
                Province = c.Province,
                PostalCode = c.PostalCode
            })
            .ToList();

        var result = PagedResult<CustomerSearchItem>.Success(items, paged.Page, paged.Size, paged.Total);
        return Result<IPagedResult<CustomerSearchItem>>.Success(result);
    }
}

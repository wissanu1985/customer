using Application.Commons.Extensions;
using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Citizens.Queries.SearchCitizens;

public sealed class RequestHandler : IRequestHandler<Request, Result<IPagedResult<CitizenSearchItem>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<IPagedResult<CitizenSearchItem>>> Handle(Request request, CancellationToken cancellationToken)
    {
        var keyword = request.Keyword?.Trim();

        var query = _unitOfWork.Repository<Citizen>().Entities;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(c =>
                c.IdCardNumber.Contains(keyword) ||
                c.FirstName.Contains(keyword) ||
                c.LastName.Contains(keyword) ||
                c.AddressLine1.Contains(keyword) ||
                c.SubDistrict.Contains(keyword) ||
                c.District.Contains(keyword) ||
                c.Province.Contains(keyword) ||
                c.PostalCode.Contains(keyword));
        }

        var paged = await query
            .OrderByDescending(c => c.CreatedDate)
            .ToPagedResultAsync(request.Page, request.Size, cancellationToken);

        var items = paged.Data
            .Select(c => new CitizenSearchItem
            {
                Id = c.Id,
                IdCardNumber = c.IdCardNumber,
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

        var result = PagedResult<CitizenSearchItem>.Success(items, paged.Page, paged.Size, paged.Total);
        return Result<IPagedResult<CitizenSearchItem>>.Success(result);
    }
}

using Application.Commons.Services;
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
    private readonly IIdCardImageStore _imageStore;

    public RequestHandler(IUnitOfWork unitOfWork, ICustomerRepository customerRepository, IIdCardImageStore imageStore)
    {
        _unitOfWork = unitOfWork;
        _customerRepository = customerRepository;
        _imageStore = imageStore;
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
                IdCardImage = null
            };

            await _unitOfWork.Repository<Customer>().AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Save encrypted image now that we have entity.Id; update the path on the entity.
            if (request.IdCardImageBytes is { Length: > 0 })
            {
                try
                {
                    var filePath = await _imageStore.SaveAsync(request.IdCardImageBytes, entity.Id, cancellationToken);
                    entity.IdCardImage = filePath;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (Exception)
                {
                    // Customer is saved, but image failed — warn the user, don't fail the whole operation
                    return Result<Response>.Success(
                        new Response(entity.Id),
                        new[] { "บันทึกข้อมูลลูกค้าเรียบร้อย แต่ไม่สามารถบันทึกรูปบัตรได้" },
                        HttpStatusCode.Created);
                }
            }

            return Result<Response>.Success(new Response(entity.Id), statusCode: HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return Result<Response>.Failure($"Failed to create customer: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}

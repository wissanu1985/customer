using Application.Commons.Services;
using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Application.Features.Customers.Queries.GetIdCardImage;

public sealed class RequestHandler : IRequestHandler<Request, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdCardImageStore _imageStore;

    public RequestHandler(IUnitOfWork unitOfWork, IIdCardImageStore imageStore)
    {
        _unitOfWork = unitOfWork;
        _imageStore = imageStore;
    }

    public async ValueTask<Result<string>> Handle(Request request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Repository<Customer>().Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer is null)
            return Result<string>.Failure("ไม่พบข้อมูลลูกค้า", HttpStatusCode.NotFound);

        if (string.IsNullOrWhiteSpace(customer.IdCardImage))
            return Result<string>.Failure("ไม่มีรูปบัตรประจำตัว", HttpStatusCode.NotFound);

        try
        {
            var imageBytes = await _imageStore.ReadAsync(customer.IdCardImage, cancellationToken);
            var contentType = DetectContentType(imageBytes);
            var base64 = Convert.ToBase64String(imageBytes);
            var dataUrl = $"data:{contentType};base64,{base64}";
            return Result<string>.Success(dataUrl);
        }
        catch (Exception)
        {
            return Result<string>.Failure("ไม่สามารถอ่านรูปบัตรได้", HttpStatusCode.InternalServerError);
        }
    }

    // Detect image MIME type from magic bytes (JPEG/PNG)
    private static string DetectContentType(byte[] bytes)
    {
        if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return "image/jpeg";
        if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return "image/png";
        return "image/jpeg"; // fallback
    }
}

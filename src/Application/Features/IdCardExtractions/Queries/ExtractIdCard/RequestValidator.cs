using FluentValidation;

namespace Application.Features.IdCardExtractions.Queries.ExtractIdCard;

public sealed class RequestValidator : AbstractValidator<Request>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
    private const int MaxImageBytes = 5 * 1024 * 1024; // 5 MB

    public RequestValidator()
    {
        RuleFor(x => x.ImageBytes)
            .NotNull().WithMessage("กรุณาเลือกไฟล์รูปภาพ")
            .Must(b => b is not null && b.Length > 0).WithMessage("ไฟล์รูปภาพไม่สามารถว่างได้")
            .Must(b => b is null || b.Length <= MaxImageBytes)
                .WithMessage("ไฟล์ต้องไม่เกิน 5MB");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("กรุณาเลือกไฟล์รูปภาพ")
            .Must(HaveAllowedExtension).WithMessage("รองรับเฉพาะไฟล์ JPEG และ PNG");
    }

    private static bool HaveAllowedExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }
}

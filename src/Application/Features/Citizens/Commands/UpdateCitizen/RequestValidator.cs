using FluentValidation;

namespace Application.Features.Citizens.Commands.UpdateCitizen;

public sealed class RequestValidator : AbstractValidator<Request>
{
    public RequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.IdCardNumber)
            .NotEmpty().WithMessage("กรุณากรอกเลขบัตรประชาชน")
            .Length(13).WithMessage("เลขบัตรประชาชนต้องมี 13 หลัก")
            .Matches(@"^\d{13}$").WithMessage("เลขบัตรประชาชนต้องเป็นตัวเลข 13 หลัก");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("กรุณากรอกชื่อ")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("กรุณากรอกนามสกุล")
            .MaximumLength(100);

        RuleFor(x => x.BirthDate)
            .NotEqual(default(DateTime)).WithMessage("กรุณากรอกวันเกิด")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("วันเกิดต้องไม่เป็นวันในอนาคต");

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("กรุณากรอกที่อยู่")
            .MaximumLength(300);

        RuleFor(x => x.SubDistrict)
            .NotEmpty().WithMessage("กรุณากรอกตำบล/แขวง")
            .MaximumLength(100);

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("กรุณากรอกอำเภอ/เขต")
            .MaximumLength(100);

        RuleFor(x => x.Province)
            .NotEmpty().WithMessage("กรุณากรอกจังหวัด")
            .MaximumLength(100);

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("กรุณากรอกรหัสไปรษณีย์")
            .Matches(@"^\d{5}$").WithMessage("รหัสไปรษณีย์ต้องเป็นตัวเลข 5 หลัก");
    }
}

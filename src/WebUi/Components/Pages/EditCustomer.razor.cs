using AntDesign;
using Application.Commons.Wrappers;
using Application.Features.Customers.Queries.GetCustomer;
using Application.Features.Customers.Queries.GetIdCardImage;
using Mediator;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using WebUi.Services;

using DetailRequest = Application.Features.Customers.Queries.GetCustomer.Request;
using ImageRequest = Application.Features.Customers.Queries.GetIdCardImage.Request;
using UpdateRequest = Application.Features.Customers.Commands.UpdateCustomer.Request;

namespace WebUi.Components.Pages;

public partial class EditCustomer
{
    [Parameter] public Guid Id { get; set; }

    [Inject] private ScopedMediator Mediator { get; set; } = default!;
    [Inject] private MessageService Message { get; set; } = default!;
    [Inject] private WebUi.Services.ErrorDialogService ErrorDialog { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private CustomerFormModel? model;
    private string? idCardImage;
    private bool submitting;
    private bool loadFailed;
    private bool validateOnChange; // Enable live re-validation only after first failed submit

    private string NotFoundMessage => $"ไม่พบข้อมูลลูกค้า (Id: {Id})";

    private List<ProvinceItem> provinces = new();
    private List<DistrictItem> districts = new();
    private List<SubDistrictItem> subDistricts = new();
    private bool provincesLoading = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            // Load customer and provinces
            var detailResult = await Mediator.Send(new DetailRequest(Id));
            var provincesResult = await Mediator.Send(new Application.Features.Locations.Queries.GetAllProvinces.Request());

            if (!detailResult.IsSuccess || detailResult.Data is null)
            {
                loadFailed = true;
                return;
            }

            if (provincesResult.IsSuccess && provincesResult.Data is not null)
            {
                provinces = provincesResult.Data
                    .Select(p => new ProvinceItem { ProvinceID = p.ProvinceID, ProvinceThai = p.ProvinceThai })
                    .ToList();
            }

            var d = detailResult.Data;
            model = new CustomerFormModel
            {
                NationalId = d.NationalId,
                FirstName = d.FirstName,
                LastName = d.LastName,
                BirthDate = d.BirthDate,
                AddressLine1 = d.AddressLine1,
                PostalCode = d.PostalCode
            };

            // Resolve province/district/subdistrict IDs from stored names so dropdowns pre-select
            await ResolveLocationSelectionsAsync(d.Province, d.District, d.SubDistrict);

            // Load ID card image for read-only display (non-fatal if it fails)
            if (!string.IsNullOrWhiteSpace(d.IdCardImage))
            {
                try
                {
                    var imageResult = await Mediator.Send(new ImageRequest(Id));
                    if (imageResult.IsSuccess)
                        idCardImage = imageResult.Data;
                }
                catch
                {
                    // Image load failure should not block editing
                }
            }
        }
        catch (Exception ex)
        {
            await ErrorDialog.ShowAsync("ไม่สามารถโหลดข้อมูลลูกค้าได้", ex);
            loadFailed = true;
        }
        finally
        {
            provincesLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    // Resolve stored location names back to IDs to pre-populate cascading dropdowns
    private async Task ResolveLocationSelectionsAsync(string provinceName, string districtName, string subDistrictName)
    {
        var province = provinces.FirstOrDefault(p => string.Equals(p.ProvinceThai, provinceName, StringComparison.OrdinalIgnoreCase));
        if (province is null) return;
        model!.ProvinceId = province.ProvinceID;

        var districtsResult = await Mediator.Send(new Application.Features.Locations.Queries.GetDistrictsByProvince.Request(province.ProvinceID));
        if (districtsResult.IsSuccess && districtsResult.Data is not null)
        {
            districts = districtsResult.Data
                .Select(d => new DistrictItem { DistrictID = d.DistrictID, DistrictThai = d.DistrictThai })
                .ToList();
        }

        var district = districts.FirstOrDefault(d => string.Equals(d.DistrictThai, districtName, StringComparison.OrdinalIgnoreCase));
        if (district is null) return;
        model.DistrictId = district.DistrictID;

        var subDistrictsResult = await Mediator.Send(new Application.Features.Locations.Queries.GetSubDistrictsByDistrict.Request(district.DistrictID));
        if (subDistrictsResult.IsSuccess && subDistrictsResult.Data is not null)
        {
            subDistricts = subDistrictsResult.Data
                .Select(s => new SubDistrictItem { TambonID = s.TambonID, TambonThai = s.TambonThai, PostalCode = s.PostalCode })
                .ToList();
        }

        var subDistrict = subDistricts.FirstOrDefault(s => string.Equals(s.TambonThai, subDistrictName, StringComparison.OrdinalIgnoreCase));
        if (subDistrict is not null)
            model.SubDistrictId = subDistrict.TambonID;
    }

    private async Task OnProvinceChangedAsync(ProvinceItem? item)
    {
        // Reset downstream selections when province changes
        districts.Clear();
        subDistricts.Clear();
        if (model is not null)
        {
            model.DistrictId = null;
            model.SubDistrictId = null;
        }

        if (item is null) return;

        try
        {
            var result = await Mediator.Send(new Application.Features.Locations.Queries.GetDistrictsByProvince.Request(item.ProvinceID));
            if (result.IsSuccess && result.Data is not null)
            {
                districts = result.Data
                    .Select(d => new DistrictItem { DistrictID = d.DistrictID, DistrictThai = d.DistrictThai })
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            await ErrorDialog.ShowAsync("ไม่สามารถโหลดข้อมูลอำเภอได้", ex);
        }
    }

    private async Task OnDistrictChangedAsync(DistrictItem? item)
    {
        subDistricts.Clear();
        if (model is not null)
            model.SubDistrictId = null;

        if (item is null) return;

        try
        {
            var result = await Mediator.Send(new Application.Features.Locations.Queries.GetSubDistrictsByDistrict.Request(item.DistrictID));
            if (result.IsSuccess && result.Data is not null)
            {
                subDistricts = result.Data
                    .Select(s => new SubDistrictItem { TambonID = s.TambonID, TambonThai = s.TambonThai, PostalCode = s.PostalCode })
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            await ErrorDialog.ShowAsync("ไม่สามารถโหลดข้อมูลตำบลได้", ex);
        }
    }

    private void OnSubDistrictChanged(SubDistrictItem? item)
    {
        // Auto-fill postal code from selected sub-district
        if (item is not null && !string.IsNullOrWhiteSpace(item.PostalCode) && model is not null)
            model.PostalCode = item.PostalCode;
    }

    // Enable live re-validation after first failed submit so errors clear on edit
    private Task OnSubmitFailedAsync(EditContext _)
    {
        validateOnChange = true;
        return Task.CompletedTask;
    }

    private async Task HandleSubmitAsync(EditContext _)
    {
        if (model is null) return;

        if (model.ProvinceId is null || model.DistrictId is null || model.SubDistrictId is null)
        {
            await Message.ErrorAsync("กรุณาเลือกจังหวัด อำเภอ และตำบลให้ครบ");
            return;
        }

        var provinceName = provinces.FirstOrDefault(p => p.ProvinceID == model.ProvinceId)?.ProvinceThai ?? string.Empty;
        var districtName = districts.FirstOrDefault(d => d.DistrictID == model.DistrictId)?.DistrictThai ?? string.Empty;
        var subDistrictName = subDistricts.FirstOrDefault(s => s.TambonID == model.SubDistrictId)?.TambonThai ?? string.Empty;

        var request = new UpdateRequest(
            Id: Id,
            NationalId: model.NationalId!,
            FirstName: model.FirstName!,
            LastName: model.LastName!,
            BirthDate: model.BirthDate!.Value,
            AddressLine1: model.AddressLine1!,
            SubDistrict: subDistrictName,
            District: districtName,
            Province: provinceName,
            PostalCode: model.PostalCode!,
            IdCardImage: null);

        submitting = true;
        try
        {
            var result = await Mediator.Send(request);
            if (result.IsSuccess)
            {
                await Message.SuccessAsync("บันทึกการแก้ไขเรียบร้อย");
                Nav.NavigateTo("/search-customer");
            }
            else
            {
                foreach (var msg in result.Messages)
                    await Message.ErrorAsync(msg);
            }
        }
        catch (FluentValidation.ValidationException vex)
        {
            foreach (var failure in vex.Errors)
                await Message.ErrorAsync(failure.ErrorMessage);
        }
        catch (Exception ex)
        {
            await Message.ErrorAsync($"เกิดข้อผิดพลาด: {ex.Message}");
        }
        finally
        {
            submitting = false;
        }
    }

    private void GoBack()
    {
        Nav.NavigateTo("/search-customer");
    }

    // Local form model with DataAnnotations for client-side validation
    private sealed class CustomerFormModel
    {
        [Required(ErrorMessage = "กรุณากรอกเลขบัตรประจำตัว")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "เลขบัตรประจำตัวต้องมี 13 หลัก")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "เลขบัตรประจำตัวต้องเป็นตัวเลข 13 หลัก")]
        public string? NationalId { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อ")]
        [StringLength(100)]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "กรุณากรอกนามสกุล")]
        [StringLength(100)]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "กรุณากรอกวันเกิด")]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "กรุณากรอกที่อยู่")]
        [StringLength(300)]
        public string? AddressLine1 { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกจังหวัด")]
        public int? ProvinceId { get; set; }

        public int? DistrictId { get; set; }

        public int? SubDistrictId { get; set; }

        [Required(ErrorMessage = "กรุณากรอกรหัสไปรษณีย์")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "รหัสไปรษณีย์ต้องเป็นตัวเลข 5 หลัก")]
        public string? PostalCode { get; set; }
    }

    // Lightweight projection DTOs for Select DataSource binding
    private sealed class ProvinceItem
    {
        public int ProvinceID { get; set; }
        public string ProvinceThai { get; set; } = string.Empty;
    }

    private sealed class DistrictItem
    {
        public int DistrictID { get; set; }
        public string DistrictThai { get; set; } = string.Empty;
    }

    private sealed class SubDistrictItem
    {
        public int TambonID { get; set; }
        public string TambonThai { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }
}

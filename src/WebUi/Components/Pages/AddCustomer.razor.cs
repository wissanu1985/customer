using AntDesign;
using Application.Commons.Wrappers;
using Application.Features.IdCardExtractions.Queries.ExtractIdCard;
using Mediator;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.ComponentModel.DataAnnotations;
using WebUi.Services;

namespace WebUi.Components.Pages;

public partial class AddCustomer
{
    [Inject] private ScopedMediator Mediator { get; set; } = default!;
    [Inject] private MessageService Message { get; set; } = default!;
    [Inject] private ILogger<AddCustomer> Logger { get; set; } = default!;
    [Inject] private WebUi.Services.ErrorDialogService ErrorDialog { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private CustomerFormModel model = new();
    private bool submitting;
    private bool validateOnChange; // Enable live re-validation only after first submit attempt

    private List<ProvinceItem> provinces = new();
    private List<DistrictItem> districts = new();
    private List<SubDistrictItem> subDistricts = new();
    private bool provincesLoading = true;
    private int selectKey; // bumped after OCR fill to force Select components to re-render
    private byte[]? uploadedImageBytes;
    private string? uploadedImagePreview;
    private bool isProcessingOcr;
    private string? ocrMessage;
    private AlertType ocrMessageType = AlertType.Info;

    // Contextual text for the loading overlay — reflects which async operation is running
    private string overlayText => isProcessingOcr ? "กำลังประมวลผล OCR..." : "กำลังบันทึกข้อมูล...";
    private const int MaxImageBytes = 5 * 1024 * 1024; // 5 MB — matches RequestValidator
    private static readonly RecyclableMemoryStreamManager StreamManager = new();

    private async Task OnImageUploadedAsync(InputFileChangeEventArgs e)
    {
        if (e.File is null) return;

        // Clear all previous form state so each upload starts fresh
        ResetForm();

        // Validate extension + size BEFORE showing loading state (spec §5: Thai messages, no API call)
        var ext = Path.GetExtension(e.File.Name).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png"))
        {
            ocrMessage = "รองรับเฉพาะไฟล์ JPEG และ PNG";
            ocrMessageType = AlertType.Error;
            return;
        }
        if (e.File.Size > MaxImageBytes)
        {
            ocrMessage = "ไฟล์ต้องไม่เกิน 5MB";
            ocrMessageType = AlertType.Error;
            return;
        }

        isProcessingOcr = true;
        StateHasChanged();

        try
        {
            // Read IBrowserFile into a pooled stream
            using var fs = e.File.OpenReadStream(maxAllowedSize: MaxImageBytes);
            using var ms = StreamManager.GetStream("idcard-upload");
            await fs.CopyToAsync(ms);
            uploadedImageBytes = ms.ToArray();
            uploadedImagePreview = $"data:{e.File.ContentType};base64,{Convert.ToBase64String(uploadedImageBytes)}";

            var result = await Mediator.Send(new Request(uploadedImageBytes, e.File.Name));
            if (!result.IsSuccess)
            {
                ocrMessage = result.Messages.FirstOrDefault() ?? "ไม่สามารถประมวลผลบัตรได้";
                ocrMessageType = AlertType.Error;
                return;
            }

            await ApplyExtractionToModel(result.Data!.Data);
            ocrMessage = "AI เติมข้อมูลอัตโนมัติ กรุณาตรวจสอบความถูกต้อง";
            ocrMessageType = AlertType.Success;
        }
        catch (Exception ex)
        {
            ocrMessage = $"เกิดข้อผิดพลาด: {ex.Message}";
            ocrMessageType = AlertType.Error;
        }
        finally
        {
            isProcessingOcr = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ApplyExtractionToModel(IdCardData data)
    {
        // Direct text fields — only fill when the value looks valid
        if (!string.IsNullOrWhiteSpace(data.NationalId) && System.Text.RegularExpressions.Regex.IsMatch(data.NationalId, @"^\d{13}$"))
            model.NationalId = data.NationalId;
        if (!string.IsNullOrWhiteSpace(data.FirstName)) model.FirstName = data.FirstName;
        if (!string.IsNullOrWhiteSpace(data.LastName)) model.LastName = data.LastName;
        if (data.BirthDate is { } bd && bd <= DateTime.Today) model.BirthDate = bd;
        if (!string.IsNullOrWhiteSpace(data.AddressLine1)) model.AddressLine1 = data.AddressLine1;

        // Cascade fuzzy match: Province → District → SubDistrict
        // StateHasChanged after each level lets AntDesign Select re-render with updated DataSource
        if (!string.IsNullOrWhiteSpace(data.ProvinceName))
        {
            Logger.LogDebug("OCR provinceName='{Raw}' normalized='{Norm}'", data.ProvinceName, NormalizeProvince(data.ProvinceName));
            Logger.LogDebug("DB provinces (normalized): {List}", string.Join(", ", provinces.Select(p => NormalizeProvince(p.ProvinceThai))));
            var provMatch = provinces
                .Where(p => string.Equals(NormalizeProvince(p.ProvinceThai), NormalizeProvince(data.ProvinceName), StringComparison.OrdinalIgnoreCase))
                .ToList();
            Logger.LogDebug("Province matches: {Count}", provMatch.Count);
            if (provMatch.Count == 1)
            {
                await OnProvinceChangedAsync(provMatch[0]);
                model.ProvinceId = provMatch[0].ProvinceID;

                if (!string.IsNullOrWhiteSpace(data.DistrictName))
                {
                    var distMatch = districts
                        .Where(d => string.Equals(NormalizeDistrict(d.DistrictThai), NormalizeDistrict(data.DistrictName), StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    if (distMatch.Count == 1)
                    {
                        await OnDistrictChangedAsync(distMatch[0]);
                        model.DistrictId = distMatch[0].DistrictID;

                        if (!string.IsNullOrWhiteSpace(data.SubDistrictName))
                        {
                            var subMatch = subDistricts
                                .Where(s => string.Equals(NormalizeSubDistrict(s.TambonThai), NormalizeSubDistrict(data.SubDistrictName), StringComparison.OrdinalIgnoreCase))
                                .ToList();
                            if (subMatch.Count == 1)
                            {
                                model.SubDistrictId = subMatch[0].TambonID;
                                OnSubDistrictChanged(subMatch[0]);
                            }
                        }
                    }
                }
            }
        }

        // PostalCode fallback: if sub-district didn't fill it, use AI's value if valid
        if (string.IsNullOrWhiteSpace(model.PostalCode)
            && !string.IsNullOrWhiteSpace(data.PostalCode)
            && System.Text.RegularExpressions.Regex.IsMatch(data.PostalCode, @"^\d{5}$"))
        {
            model.PostalCode = data.PostalCode;
        }

        // Force AntDesign Select to re-create so programmatic value is displayed correctly
        selectKey++;
    }

    private static string NormalizeProvince(string s) => StripPrefixes(s, "จังหวัด", "จ.");
    private static string NormalizeDistrict(string s) => StripPrefixes(s, "อำเภอ", "เขต", "อ.");
    private static string NormalizeSubDistrict(string s) => StripPrefixes(s, "ตำบล", "แขวง", "ต.");
    private static string StripPrefixes(string s, params string[] prefixes)
    {
        var t = s.Trim();
        foreach (var p in prefixes)
            if (t.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                t = t[p.Length..].Trim();
        return t;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        try
        {
            var result = await Mediator.Send(new Application.Features.Locations.Queries.GetAllProvinces.Request());
            if (result.IsSuccess && result.Data is not null)
            {
                provinces = result.Data
                    .Select(p => new ProvinceItem { ProvinceID = p.ProvinceID, ProvinceThai = p.ProvinceThai })
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            await ErrorDialog.ShowAsync("ไม่สามารถโหลดข้อมูลจังหวัดได้", ex);
        }
        finally
        {
            provincesLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnProvinceChangedAsync(ProvinceItem? item)
    {
        // Reset downstream selections when province changes
        districts.Clear();
        subDistricts.Clear();
        model.DistrictId = null;
        model.SubDistrictId = null;

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
        if (item is not null && !string.IsNullOrWhiteSpace(item.PostalCode))
        {
            model.PostalCode = item.PostalCode;
        }
    }

    // Enable live re-validation after first failed submit so errors clear on edit
    private Task OnSubmitFailedAsync(EditContext _)
    {
        validateOnChange = true;
        return Task.CompletedTask;
    }

    private async Task HandleSubmitAsync(EditContext _)
    {
        if (model.ProvinceId is null || model.DistrictId is null || model.SubDistrictId is null)
        {
            await Message.ErrorAsync("กรุณาเลือกจังหวัด อำเภอ และตำบลให้ครบ");
            return;
        }

        var provinceName = provinces.FirstOrDefault(p => p.ProvinceID == model.ProvinceId)?.ProvinceThai ?? string.Empty;
        var districtName = districts.FirstOrDefault(d => d.DistrictID == model.DistrictId)?.DistrictThai ?? string.Empty;
        var subDistrictName = subDistricts.FirstOrDefault(s => s.TambonID == model.SubDistrictId)?.TambonThai ?? string.Empty;

        var request = new Application.Features.Customers.Commands.CreateCustomer.Request(
            NationalId: model.NationalId!,
            FirstName: model.FirstName!,
            LastName: model.LastName!,
            BirthDate: model.BirthDate!.Value,
            AddressLine1: model.AddressLine1!,
            SubDistrict: subDistrictName,
            District: districtName,
            Province: provinceName,
            PostalCode: model.PostalCode!,
            IdCardImage: null,
            IdCardImageBytes: uploadedImageBytes);

        submitting = true;
        try
        {
            var result = await Mediator.Send(request);
            if (result.IsSuccess)
            {
                await Message.SuccessAsync("บันทึกข้อมูลลูกค้าเรียบร้อย");
                ResetForm();
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

    private void ResetForm()
    {
        model = new CustomerFormModel();
        districts.Clear();
        subDistricts.Clear();
        validateOnChange = false; // Back to validate-on-submit only for the fresh form
        uploadedImageBytes = null;
        uploadedImagePreview = null;
        ocrMessage = null;
        selectKey++; // Force AntDesign Select components to re-render with cleared values
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

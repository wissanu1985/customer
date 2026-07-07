using AntDesign;
using AntDesign.TableModels;
using Application.Commons.Wrappers;
using Application.Features.Customers.Queries.GetCustomer;
using Application.Features.Customers.Queries.GetIdCardImage;
using Application.Features.Customers.Queries.SearchCustomers;
using Mediator;
using Microsoft.AspNetCore.Components;
using WebUi.Services;

using SearchRequest = Application.Features.Customers.Queries.SearchCustomers.Request;
using DetailRequest = Application.Features.Customers.Queries.GetCustomer.Request;
using ImageRequest = Application.Features.Customers.Queries.GetIdCardImage.Request;

namespace WebUi.Components.Pages;

public partial class SearchCustomer
{
    [Inject] private ScopedMediator Mediator { get; set; } = default!;
    [Inject] private MessageService Message { get; set; } = default!;
    [Inject] private WebUi.Services.ErrorDialogService ErrorDialog { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    // Search filters
    private string nationalId = string.Empty;
    private string firstName = string.Empty;
    private string lastName = string.Empty;
    private string postalCode = string.Empty;

    private int? selectedProvinceId;
    private int? selectedDistrictId;
    private int? selectedSubDistrictId;

    private List<ProvinceItem> provinces = new();
    private List<DistrictItem> districts = new();
    private List<SubDistrictItem> subDistricts = new();
    private bool provincesLoading = true;

    // Result state
    private List<CustomerSearchItem> rows = new();
    private int total;
    private int pageIndex = 1;
    private int pageSize = 10;
    private bool loading;

    // Detail drawer state
    private bool detailVisible;
    private bool detailLoading;
    private CustomerDetail? detail;
    private string? detailIdCardImage;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await LoadProvincesAsync();
        await SearchAsync();
    }

    private async Task LoadProvincesAsync()
    {
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
        selectedDistrictId = null;
        selectedSubDistrictId = null;
        postalCode = string.Empty;

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
        selectedSubDistrictId = null;
        postalCode = string.Empty;

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
        // Auto-fill and lock postal code when sub-district is selected
        if (item is not null && !string.IsNullOrWhiteSpace(item.PostalCode))
            postalCode = item.PostalCode;
        else
            postalCode = string.Empty;
    }

    private async Task OnSearchClickAsync()
    {
        pageIndex = 1;
        await SearchAsync();
    }

    private async Task OnTableChangeAsync(QueryModel<CustomerSearchItem> query)
    {
        pageIndex = query.PageIndex;
        pageSize = query.PageSize;
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        loading = true;
        try
        {
            var provinceName = selectedProvinceId is not null
                ? provinces.FirstOrDefault(p => p.ProvinceID == selectedProvinceId)?.ProvinceThai
                : null;
            var districtName = selectedDistrictId is not null
                ? districts.FirstOrDefault(d => d.DistrictID == selectedDistrictId)?.DistrictThai
                : null;
            var subDistrictName = selectedSubDistrictId is not null
                ? subDistricts.FirstOrDefault(s => s.TambonID == selectedSubDistrictId)?.TambonThai
                : null;

            var result = await Mediator.Send(new SearchRequest(
                NationalId: NullIfEmpty(nationalId),
                FirstName: NullIfEmpty(firstName),
                LastName: NullIfEmpty(lastName),
                Province: provinceName,
                District: districtName,
                SubDistrict: subDistrictName,
                PostalCode: NullIfEmpty(postalCode),
                Page: pageIndex,
                Size: pageSize));

            if (result.IsSuccess && result.Data is not null)
            {
                rows = result.Data.Data.ToList();
                total = result.Data.Total;
            }
            else
            {
                rows.Clear();
                total = 0;
                foreach (var msg in result.Messages)
                    await Message.WarningAsync(msg);
            }
        }
        catch (Exception ex)
        {
            await ErrorDialog.ShowAsync("ไม่สามารถค้นหาข้อมูลได้", ex);
        }
        finally
        {
            loading = false;
            // Required when SearchAsync is invoked from OnAfterRenderAsync, which has no
            // auto StateHasChanged after it returns (unlike event handlers like OnSearchClickAsync).
            await InvokeAsync(StateHasChanged);
        }
    }

    private void ResetFilters()
    {
        nationalId = string.Empty;
        firstName = string.Empty;
        lastName = string.Empty;
        postalCode = string.Empty;
        selectedProvinceId = null;
        selectedDistrictId = null;
        selectedSubDistrictId = null;
        districts.Clear();
        subDistricts.Clear();
    }

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task OpenDetailAsync(Guid id)
    {
        detailVisible = true;
        detailLoading = true;
        detail = null;
        detailIdCardImage = null;
        await InvokeAsync(StateHasChanged);

        try
        {
            var result = await Mediator.Send(new DetailRequest(id));
            if (result.IsSuccess && result.Data is not null)
            {
                detail = result.Data;

                // Load ID card image in parallel — non-fatal if it fails
                if (!string.IsNullOrWhiteSpace(detail.IdCardImage))
                {
                    try
                    {
                        var imageResult = await Mediator.Send(new ImageRequest(id));
                        if (imageResult.IsSuccess)
                            detailIdCardImage = imageResult.Data;
                    }
                    catch
                    {
                        // Image load failure should not block the detail view
                    }
                }
            }
            else
                await Message.WarningAsync(result.Messages.Any() ? string.Join(" ", result.Messages) : "ไม่พบข้อมูล");
        }
        catch (Exception ex)
        {
            await ErrorDialog.ShowAsync("ไม่สามารถโหลดรายละเอียดได้", ex);
        }
        finally
        {
            detailLoading = false;
        }
    }

    private void OnDetailClose()
    {
        detailVisible = false;
        detail = null;
        detailIdCardImage = null;
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

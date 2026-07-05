# Citizen Management UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build List, Detail, Edit pages for citizens plus ID-card image upload — completing the citizen CRUD flow in the Blazor Server UI.

**Architecture:** Extract a shared `CitizenForm` component (cascading province/district/subdistrict dropdowns + validation) used by both Add and Edit pages. List page uses AntDesign Table with server-side pagination via existing `SearchCitizens` query. Detail page uses AntDesign Descriptions. Upload uses AntDesign `<Upload>` with JS interop to read file as base64 data URL stored in `Citizen.IdCardImage` column.

**Tech Stack:** .NET 10, Blazor Server, AntDesign Blazor 1.6.2, Mediator (source-generated), FluentValidation, xUnit + bUnit.web + NSubstitute (test project)

---

## File Structure

| File | Responsibility | Action |
|------|---------------|--------|
| `tests/WebUi.Tests/WebUi.Tests.csproj` | bUnit test project | Create |
| `tests/WebUi.Tests/_Imports.razor` | Global usings for test project | Create |
| `tests/WebUi.Tests/Shared/CitizenFormTests.cs` | Smoke tests for CitizenForm | Create |
| `tests/WebUi.Tests/Pages/ListCitizensTests.cs` | Smoke tests for ListCitizens | Create |
| `tests/WebUi.Tests/Pages/CitizenDetailTests.cs` | Smoke tests for CitizenDetail | Create |
| `tests/WebUi.Tests/Pages/EditCitizenTests.cs` | Smoke tests for EditCitizen | Create |
| `src/WebUi/Components/Shared/CitizenFormModel.cs` | Shared form model with DataAnnotations | Create |
| `src/WebUi/Components/Shared/CitizenForm.razor` | Shared form UI (cascading dropdowns + upload) | Create |
| `src/WebUi/Components/Shared/CitizenForm.razor.cs` | CitizenForm code-behind (dropdown loading, submit) | Create |
| `src/WebUi/Components/Pages/AddCitizen.razor` | Thin wrapper around CitizenForm | Modify |
| `src/WebUi/Components/Pages/ListCitizens.razor` | Search + paged AntDesign Table | Create |
| `src/WebUi/Components/Pages/CitizenDetail.razor` | Read-only Descriptions view | Create |
| `src/WebUi/Components/Pages/EditCitizen.razor` | Load citizen → CitizenForm → UpdateCitizen | Create |
| `src/WebUi/wwwroot/js/citizen-app.js` | JS interop: read blob as data URL | Create |
| `src/WebUi/Components/App.razor` | Add script reference for citizen-app.js | Modify |
| `src/WebUi/Components/Layout/NavMenu.razor` | Add "รายชื่อพลเมือง" link | Modify |

**Routes:**
- `/citizens` — ListCitizens
- `/citizens/{Id:guid}` — CitizenDetail
- `/citizens/{Id:guid}/edit` — EditCitizen
- `/add-citizen` — AddCitizen (existing, refactored)

---

## Task 1: Set up bUnit test project

**Files:**
- Create: `tests/WebUi.Tests/WebUi.Tests.csproj`
- Create: `tests/WebUi.Tests/_Imports.razor`
- Modify: `Citizen.slnx` (add test project)

- [ ] **Step 1: Create test project directory and csproj**

```bash
mkdir tests\WebUi.Tests
```

Create `tests/WebUi.Tests/WebUi.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="bunit.web" Version="1.36.0" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.13.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.0.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\WebUi\WebUi.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create _Imports.razor for test project**

Create `tests/WebUi.Tests/_Imports.razor`:

```razor
@using AntDesign
@using Bunit
@using Microsoft.AspNetCore.Components
@using Microsoft.Extensions.DependencyInjection
@using NSubstitute
@using NSubstitute.ClearExtensions
@using WebUi.Components.Pages
@using WebUi.Components.Shared
@using WebUi.Components.Layout
@using Xunit
```

- [ ] **Step 3: Add test project to solution**

Run:
```bash
dotnet sln "E:\WorkPlace\test\Citizen\Citizen.slnx" add "tests\WebUi.Tests\WebUi.Tests.csproj"
```

Expected: `Project ... was added.`

- [ ] **Step 4: Restore and build test project**

Run:
```bash
dotnet build "tests\WebUi.Tests\WebUi.Tests.csproj" --nologo
```

Expected: Build succeeded with 0 errors. If bunit.web 1.36.0 is incompatible with net10.0, try the latest stable: `dotnet add package bunit.web` without version pin.

- [ ] **Step 5: Commit**

```bash
git add tests/WebUi.Tests/ Citizen.slnx
git commit -m "test: add bUnit test project for WebUi"
```

---

## Task 2: Create CitizenFormModel (shared form model)

**Files:**
- Create: `src/WebUi/Components/Shared/CitizenFormModel.cs`
- Test: `tests/WebUi.Tests/Shared/CitizenFormTests.cs` (minimal — just verifies the model can be instantiated)

- [ ] **Step 1: Write a failing test**

Create `tests/WebUi.Tests/Shared/CitizenFormTests.cs`:

```csharp
namespace WebUi.Tests.Shared;

public class CitizenFormModelTests
{
    [Fact]
    public void NewModel_HasNullDefaults_ForOptionalFields()
    {
        var model = new CitizenFormModel();
        Assert.Null(model.IdCardNumber);
        Assert.Null(model.ProvinceId);
        Assert.Null(model.DistrictId);
        Assert.Null(model.SubDistrictId);
        Assert.Null(model.IdCardImage);
        Assert.Null(model.InitialProvinceName);
    }

    [Fact]
    public void NewModel_HasEmptyString_ForLocationNames()
    {
        var model = new CitizenFormModel();
        Assert.Equal("", model.Province);
        Assert.Equal("", model.District);
        Assert.Equal("", model.SubDistrict);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:
```bash
dotnet test "tests\WebUi.Tests\WebUi.Tests.csproj" --filter "CitizenFormModelTests" --nologo
```

Expected: FAIL — `CitizenFormModel` not found.

- [ ] **Step 3: Create CitizenFormModel**

Create `src/WebUi/Components/Shared/CitizenFormModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace WebUi.Components.Shared;

/// <summary>
/// Shared form model used by CitizenForm for both Add and Edit pages.
/// Location IDs drive the cascading dropdowns; location names are populated
/// by CitizenForm when a dropdown selection changes and are sent to the
/// Create/Update commands. InitialXxxName fields are set by EditCitizen
/// to pre-select the correct dropdown items during initialization.
/// </summary>
public sealed class CitizenFormModel
{
    [Required(ErrorMessage = "กรุณากรอกเลขบัตรประชาชน")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "เลขบัตรประชาชนต้องมี 13 หลัก")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "เลขบัตรประชาชนต้องเป็นตัวเลข 13 หลัก")]
    public string? IdCardNumber { get; set; }

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
    public string Province { get; set; } = "";

    public int? DistrictId { get; set; }
    public string District { get; set; } = "";

    public int? SubDistrictId { get; set; }
    public string SubDistrict { get; set; } = "";

    [Required(ErrorMessage = "กรุณากรอกรหัสไปรษณีย์")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "รหัสไปรษณีย์ต้องเป็นตัวเลข 5 หลัก")]
    public string? PostalCode { get; set; }

    /// <summary>Base64 data URL of the uploaded ID card image (e.g. "data:image/jpeg;base64,...").</summary>
    public string? IdCardImage { get; set; }

    // --- Edit-mode pre-population: set by EditCitizen so CitizenForm can find the right IDs ---

    public string? InitialProvinceName { get; set; }
    public string? InitialDistrictName { get; set; }
    public string? InitialSubDistrictName { get; set; }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run:
```bash
dotnet test "tests\WebUi.Tests\WebUi.Tests.csproj" --filter "CitizenFormModelTests" --nologo
```

Expected: PASS — 2 tests.

- [ ] **Step 5: Commit**

```bash
git add src/WebUi/Components/Shared/CitizenFormModel.cs tests/WebUi.Tests/Shared/CitizenFormTests.cs
git commit -m "feat: add shared CitizenFormModel with validation annotations"
```

---

## Task 3: Create CitizenForm shared component (cascading dropdowns, no upload yet)

**Files:**
- Create: `src/WebUi/Components/Shared/CitizenForm.razor`
- Create: `src/WebUi/Components/Shared/CitizenForm.razor.cs`
- Test: `tests/WebUi.Tests/Shared/CitizenFormTests.cs` (extend)

- [ ] **Step 1: Write failing test — CitizenForm renders with empty model**

Append to `tests/WebUi.Tests/Shared/CitizenFormTests.cs`:

```csharp
using Application.Commons.Wrappers;
using Application.Features.Locations.Queries.GetAllProvinces;
using Application.Features.Locations.Queries.GetDistrictsByProvince;
using Application.Features.Locations.Queries.GetSubDistrictsByDistrict;
using Mediator;
using Microsoft.AspNetCore.Components.Forms;

namespace WebUi.Tests.Shared;

public class CitizenFormModelTests
{
    [Fact]
    public void NewModel_HasNullDefaults_ForOptionalFields()
    {
        var model = new CitizenFormModel();
        Assert.Null(model.IdCardNumber);
        Assert.Null(model.ProvinceId);
        Assert.Null(model.DistrictId);
        Assert.Null(model.SubDistrictId);
        Assert.Null(model.IdCardImage);
        Assert.Null(model.InitialProvinceName);
    }

    [Fact]
    public void NewModel_HasEmptyString_ForLocationNames()
    {
        var model = new CitizenFormModel();
        Assert.Equal("", model.Province);
        Assert.Equal("", model.District);
        Assert.Equal("", model.SubDistrict);
    }
}

public class CitizenFormRenderTests
{
    [Fact]
    public void Renders_WithFormAndLabels_ForEmptyModel()
    {
        using var ctx = new TestContext();
        ctx.Services.AddAntDesign();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllProvinces.Request>())
                .Returns(ValueTask.FromResult(Result<List<GetAllProvinces.Response>>.Success(new())));
        ctx.Services.AddScoped(_ => mediator);

        var cut = ctx.RenderComponent<CitizenForm>(parameters => parameters
            .Add(p => p.Model, new CitizenFormModel())
            .Add(p => p.SubmitText, "บันทึก"));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("เลขบัตรประชาชน", cut.Markup);
            Assert.Contains("จังหวัด", cut.Markup);
            Assert.Contains("บันทึก", cut.Markup);
        });
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:
```bash
dotnet test "tests\WebUi.Tests\WebUi.Tests.csproj" --filter "CitizenFormRenderTests" --nologo
```

Expected: FAIL — `CitizenForm` component not found.

- [ ] **Step 3: Create CitizenForm.razor**

Create `src/WebUi/Components/Shared/CitizenForm.razor`:

```razor
@using System.ComponentModel.DataAnnotations

<Form Model="@Model" Layout="@FormLayout.Vertical" OnFinish="HandleValidSubmitAsync">
    <FormItem Label="เลขบัตรประชาชน" Name="IdCardNumber">
        <Input @bind-Value="@context.IdCardNumber" Placeholder="กรอกเลขบัตรประชาชน 13 หลัก" MaxLength="13" />
    </FormItem>

    <Row Gutter="(16, 16)">
        <Col Xs="24" Sm="12">
            <FormItem Label="ชื่อ" Name="FirstName">
                <Input @bind-Value="@context.FirstName" Placeholder="กรอกชื่อ" />
            </FormItem>
        </Col>
        <Col Xs="24" Sm="12">
            <FormItem Label="นามสกุล" Name="LastName">
                <Input @bind-Value="@context.LastName" Placeholder="กรอกนามสกุล" />
            </FormItem>
        </Col>
    </Row>

    <FormItem Label="วันเกิด" Name="BirthDate">
        <DatePicker @bind-Value="@context.BirthDate" Picker="@DatePickerType.Date"
                    DisabledDate="@(d => d > DateTime.Today)" Style="width: 100%" />
    </FormItem>

    <FormItem Label="ที่อยู่ (บ้านเลขที่ ถนน ซอย)" Name="AddressLine1">
        <TextArea @bind-Value="@context.AddressLine1" Placeholder="กรอกที่อยู่" Rows="2" />
    </FormItem>

    <Row Gutter="(16, 16)">
        <Col Xs="24" Sm="12">
            <FormItem Label="จังหวัด" Name="ProvinceId">
                <Select TItem="ProvinceItem" TItemValue="int?"
                        DataSource="@_provinces"
                        ItemLabel="p => p.ProvinceThai"
                        ItemValue="p => (int?)p.ProvinceID"
                        @bind-Value="@Model.ProvinceId"
                        Placeholder="เลือกจังหวัด"
                        OnSelectedItemChanged="OnProvinceChangedAsync"
                        AllowClear="true"
                        Style="width: 100%" />
            </FormItem>
        </Col>
        <Col Xs="24" Sm="12">
            <FormItem Label="อำเภอ/เขต" Name="DistrictId">
                <Select TItem="DistrictItem" TItemValue="int?"
                        DataSource="@_districts"
                        ItemLabel="d => d.DistrictThai"
                        ItemValue="d => (int?)d.DistrictID"
                        @bind-Value="@Model.DistrictId"
                        Placeholder="เลือกอำเภอ/เขต"
                        OnSelectedItemChanged="OnDistrictChangedAsync"
                        Disabled="@(Model.ProvinceId is null)"
                        AllowClear="true"
                        Style="width: 100%" />
            </FormItem>
        </Col>
    </Row>

    <Row Gutter="(16, 16)">
        <Col Xs="24" Sm="12">
            <FormItem Label="ตำบล/แขวง" Name="SubDistrictId">
                <Select TItem="SubDistrictItem" TItemValue="int?"
                        DataSource="@_subDistricts"
                        ItemLabel="s => s.TambonThai"
                        ItemValue="s => (int?)s.TambonID"
                        @bind-Value="@Model.SubDistrictId"
                        Placeholder="เลือกตำบล/แขวง"
                        OnSelectedItemChanged="OnSubDistrictChanged"
                        Disabled="@(Model.DistrictId is null)"
                        AllowClear="true"
                        Style="width: 100%" />
            </FormItem>
        </Col>
        <Col Xs="24" Sm="12">
            <FormItem Label="รหัสไปรษณีย์" Name="PostalCode">
                <Input @bind-Value="@context.PostalCode" Placeholder="รหัสไปรษณีย์ 5 หลัก" MaxLength="5" />
            </FormItem>
        </Col>
    </Row>

    @if (Model.IdCardImage is not null)
    {
        <FormItem Label="รูปบัตรประชาชน">
            <Image Src="@Model.IdCardImage" Width="200" />
        </FormItem>
    }

    <FormItem>
        <Space>
            <SpaceItem>
                <Button Type="@ButtonType.Primary" HtmlType="submit" Loading="@Loading">
                    @SubmitText
                </Button>
            </SpaceItem>
            @if (OnCancel.HasDelegate)
            {
                <SpaceItem>
                    <Button Type="@ButtonType.Default" OnClick="HandleCancel">ยกเลิก</Button>
                </SpaceItem>
            }
        </Space>
    </FormItem>
</Form>

@code {
    // Lightweight projection DTOs for Select DataSource binding
    internal sealed class ProvinceItem
    {
        public int ProvinceID { get; set; }
        public string ProvinceThai { get; set; } = string.Empty;
    }

    internal sealed class DistrictItem
    {
        public int DistrictID { get; set; }
        public string DistrictThai { get; set; } = string.Empty;
    }

    internal sealed class SubDistrictItem
    {
        public int TambonID { get; set; }
        public string TambonThai { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 4: Create CitizenForm.razor.cs (code-behind)**

Create `src/WebUi/Components/Shared/CitizenForm.razor.cs`:

```csharp
using Application.Commons.Wrappers;
using Application.Features.Locations.Queries.GetAllProvinces;
using Application.Features.Locations.Queries.GetDistrictsByProvince;
using Application.Features.Locations.Queries.GetSubDistrictsByDistrict;
using Mediator;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace WebUi.Components.Shared;

public partial class CitizenForm
{
    [Inject] private IMediator Mediator { get; set; } = default!;

    [Parameter] public CitizenFormModel Model { get; set; } = new();
    [Parameter] public EventCallback<CitizenFormModel> OnValidSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public string SubmitText { get; set; } = "บันทึก";
    [Parameter] public bool Loading { get; set; }

    private readonly List<ProvinceItem> _provinces = new();
    private readonly List<DistrictItem> _districts = new();
    private readonly List<SubDistrictItem> _subDistricts = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadProvincesAsync();

        // Edit-mode pre-population: match initial names to IDs
        if (Model.InitialProvinceName is not null)
        {
            var p = _provinces.FirstOrDefault(x => x.ProvinceThai == Model.InitialProvinceName);
            if (p is not null)
            {
                Model.ProvinceId = p.ProvinceID;
                await LoadDistrictsAsync(p.ProvinceID);
            }
        }

        if (Model.InitialDistrictName is not null)
        {
            var d = _districts.FirstOrDefault(x => x.DistrictThai == Model.InitialDistrictName);
            if (d is not null)
            {
                Model.DistrictId = d.DistrictID;
                await LoadSubDistrictsAsync(d.DistrictID);
            }
        }

        if (Model.InitialSubDistrictName is not null)
        {
            var s = _subDistricts.FirstOrDefault(x => x.TambonThai == Model.InitialSubDistrictName);
            if (s is not null)
            {
                Model.SubDistrictId = s.TambonID;
                if (!string.IsNullOrWhiteSpace(s.PostalCode))
                    Model.PostalCode = s.PostalCode;
            }
        }
    }

    private async Task LoadProvincesAsync()
    {
        var result = await Mediator.Send(new GetAllProvinces.Request());
        if (result.IsSuccess && result.Data is not null)
        {
            _provinces.Clear();
            _provinces.AddRange(result.Data.Select(p => new ProvinceItem
            {
                ProvinceID = p.ProvinceID,
                ProvinceThai = p.ProvinceThai
            }));
        }
    }

    private async Task LoadDistrictsAsync(int provinceId)
    {
        var result = await Mediator.Send(new GetDistrictsByProvince.Request(provinceId));
        if (result.IsSuccess && result.Data is not null)
        {
            _districts.Clear();
            _districts.AddRange(result.Data.Select(d => new DistrictItem
            {
                DistrictID = d.DistrictID,
                DistrictThai = d.DistrictThai
            }));
        }
    }

    private async Task LoadSubDistrictsAsync(int districtId)
    {
        var result = await Mediator.Send(new GetSubDistrictsByDistrict.Request(districtId));
        if (result.IsSuccess && result.Data is not null)
        {
            _subDistricts.Clear();
            _subDistricts.AddRange(result.Data.Select(s => new SubDistrictItem
            {
                TambonID = s.TambonID,
                TambonThai = s.TambonThai,
                PostalCode = s.PostalCode
            }));
        }
    }

    private async Task OnProvinceChangedAsync(ProvinceItem? item)
    {
        _districts.Clear();
        _subDistricts.Clear();
        Model.DistrictId = null;
        Model.SubDistrictId = null;
        Model.Province = item?.ProvinceThai ?? "";
        Model.District = "";
        Model.SubDistrict = "";

        if (item is null) return;
        await LoadDistrictsAsync(item.ProvinceID);
    }

    private async Task OnDistrictChangedAsync(DistrictItem? item)
    {
        _subDistricts.Clear();
        Model.SubDistrictId = null;
        Model.District = item?.DistrictThai ?? "";
        Model.SubDistrict = "";

        if (item is null) return;
        await LoadSubDistrictsAsync(item.DistrictID);
    }

    private void OnSubDistrictChanged(SubDistrictItem? item)
    {
        Model.SubDistrict = item?.TambonThai ?? "";
        if (item is not null && !string.IsNullOrWhiteSpace(item.PostalCode))
            Model.PostalCode = item.PostalCode;
    }

    private async Task HandleValidSubmitAsync(EditContext _)
    {
        await OnValidSubmit.InvokeAsync(Model);
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
    }
}
```

- [ ] **Step 5: Build to verify no compile errors**

Run:
```bash
dotnet build "src/WebUi/WebUi.csproj" --nologo
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 6: Run test to verify it passes**

Run:
```bash
dotnet test "tests/WebUi.Tests\WebUi.Tests.csproj" --filter "CitizenFormRenderTests" --nologo
```

Expected: PASS — 1 test. (If the test times out waiting for async rendering, increase the timeout in `WaitForAssertion` or use `cut.WaitForState(() => cut.Find("form") is not null)`.)

- [ ] **Step 7: Commit**

```bash
git add src/WebUi/Components/Shared/CitizenForm.razor src/WebUi/Components/Shared/CitizenForm.razor.cs tests/WebUi.Tests/Shared/CitizenFormTests.cs
git commit -m "feat: add shared CitizenForm component with cascading location dropdowns"
```

---

## Task 4: Refactor AddCitizen.razor to use CitizenForm

**Files:**
- Modify: `src/WebUi/Components/Pages/AddCitizen.razor` (replace entire file — it becomes a thin wrapper)
- Test: `tests/WebUi.Tests/Pages/AddCitizenTests.cs` (new)

- [ ] **Step 1: Write failing test — AddCitizen renders CitizenForm**

Create `tests/WebUi.Tests/Pages/AddCitizenTests.cs`:

```csharp
using Application.Commons.Wrappers;
using Application.Features.Citizens.Commands.CreateCitizen;
using Application.Features.Locations.Queries.GetAllProvinces;
using Mediator;

namespace WebUi.Tests.Pages;

public class AddCitizenTests
{
    [Fact]
    public void Renders_PageTitle_AndForm()
    {
        using var ctx = new TestContext();
        ctx.Services.AddAntDesign();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllProvinces.Request>())
                .Returns(ValueTask.FromResult(Result<List<GetAllProvinces.Response>>.Success(new())));
        ctx.Services.AddScoped(_ => mediator);

        var cut = ctx.RenderComponent<AddCitizen>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("เพิ่มพลเมือง", cut.Markup);
            Assert.Contains("เลขบัตรประชาชน", cut.Markup);
        });
    }
}
```

- [ ] **Step 2: Run test — it should still pass with the old AddCitizen (sanity check)**

Run:
```bash
dotnet test "tests/WebUi.Tests\WebUi.Tests.csproj" --filter "AddCitizenTests" --nologo
```

Expected: PASS (old AddCitizen already has these labels). This confirms the test is valid before we refactor.

- [ ] **Step 3: Replace AddCitizen.razor with thin wrapper**

Replace entire contents of `src/WebUi/Components/Pages/AddCitizen.razor`:

```razor
@page "/add-citizen"
@rendermode InteractiveServer
@using Application.Commons.Wrappers
@using Mediator
@inject IMediator Mediator
@inject MessageService Message
@inject NavigationManager Nav

<PageTitle>เพิ่มพลเมือง</PageTitle>

<h1>เพิ่มพลเมือง</h1>

<CitizenForm Model="@_model"
             OnValidSubmit="HandleSubmitAsync"
             SubmitText="บันทึก"
             Loading="@_submitting" />

@code {
    private CitizenFormModel _model = new();
    private bool _submitting;

    private async Task HandleSubmitAsync(CitizenFormModel form)
    {
        if (form.ProvinceId is null || form.DistrictId is null || form.SubDistrictId is null)
        {
            await Message.ErrorAsync("กรุณาเลือกจังหวัด อำเภอ และตำบลให้ครบ");
            return;
        }

        var request = new Application.Features.Citizens.Commands.CreateCitizen.Request(
            IdCardNumber: form.IdCardNumber!,
            FirstName: form.FirstName!,
            LastName: form.LastName!,
            BirthDate: form.BirthDate!.Value,
            AddressLine1: form.AddressLine1!,
            SubDistrict: form.SubDistrict,
            District: form.District,
            Province: form.Province,
            PostalCode: form.PostalCode!,
            IdCardImage: form.IdCardImage);

        _submitting = true;
        try
        {
            var result = await Mediator.Send(request);
            if (result.IsSuccess)
            {
                await Message.SuccessAsync("บันทึกข้อมูลพลเมืองเรียบร้อย");
                _model = new CitizenFormModel();
                Nav.NavigateTo("/citizens");
            }
            else
            {
                foreach (var msg in result.Messages)
                    await Message.ErrorAsync(msg);
            }
        }
        finally
        {
            _submitting = false;
        }
    }
}
```

- [ ] **Step 4: Build to verify no compile errors**

Run:
```bash
dotnet build "src/WebUi/WebUi.csproj" --nologo
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Run test to verify it passes**

Run:
```bash
dotnet test "tests/WebUi.Tests\WebUi.Tests.csproj" --filter "AddCitizenTests" --nologo
```

Expected: PASS — 1 test.

- [ ] **Step 6: Commit**

```bash
git add src/WebUi/Components/Pages/AddCitizen.razor tests/WebUi.Tests/Pages/AddCitizenTests.cs
git commit -m "refactor: AddCitizen now uses shared CitizenForm component"
```

---

## Task 5: Create ListCitizens.razor (search + paged AntDesign Table)

**Files:**
- Create: `src/WebUi/Components/Pages/ListCitizens.razor`
- Test: `tests/WebUi.Tests/Pages/ListCitizensTests.cs`

- [ ] **Step 1: Write failing test**

Create `tests/WebUi.Tests/Pages/ListCitizensTests.cs`:

```csharp
using Application.Commons.Wrappers;
using Application.Features.Citizens.Queries.SearchCitizens;
using Mediator;

namespace WebUi.Tests.Pages;

public class ListCitizensTests
{
    [Fact]
    public void Renders_Title_AndSearchInput()
    {
        using var ctx = new TestContext();
        ctx.Services.AddAntDesign();
        var mediator = Substitute.For<IMediator>();
        var emptyPaged = PagedResult<CitizenSearchItem>.Success(
            new List<CitizenSearchItem>(), page: 1, size: 10, total: 0);
        mediator.Send(Arg.Any<SearchCitizens.Request>())
                .Returns(ValueTask.FromResult(Result<IPagedResult<CitizenSearchItem>>.Success(emptyPaged)));
        ctx.Services.AddScoped(_ => mediator);

        var cut = ctx.RenderComponent<ListCitizens>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("รายชื่อพลเมือง", cut.Markup);
        });
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:
```bash
dotnet test "tests/WebUi.Tests\WebUi.Tests.csproj" --filter "ListCitizensTests" --nologo
```

Expected: FAIL — `ListCitizens` not found.

- [ ] **Step 3: Create ListCitizens.razor**

Create `src/WebUi/Components/Pages/ListCitizens.razor`:

```razor
@page "/citizens"
@rendermode InteractiveServer
@using Application.Commons.Wrappers
@using Application.Features.Citizens.Queries.SearchCitizens
@using Mediator
@inject IMediator Mediator
@inject NavigationManager Nav

<PageTitle>รายชื่อพลเมือง</PageTitle>

<h1>รายชื่อพลเมือง</h1>

<Space Direction="DirectionVType.Vertical" Style="width: 100%">
    <SpaceItem>
        <Search Placeholder="ค้นหาด้วยเลขบัตร ชื่อ นามสกุล ที่อยู่..."
                @bind-Value="_keyword"
                OnSearch="SearchAsync"
                Style="width: 400px"
                EnterButton />
    </SpaceItem>
    <SpaceItem>
        @if (_result is null)
        {
            <Spin Spinning />
        }
        else
        {
            <Table TItem="CitizenSearchItem"
                   DataSource="@_result.Data"
                   Loading="@_loading"
                   Total="@_result.Total"
                   PageIndex="@_page"
                   PageSize="@_size"
                   OnChange="HandleTableChangeAsync"
                   RowKey="r => r.Id">
                <Column TData="string" Title="เลขบัตร" @bind-Field="@context.IdCardNumber" Width="140" />
                <Column TData="string" Title="ชื่อ" @bind-Field="@context.FirstName" />
                <Column TData="string" Title="นามสกุล" @bind-Field="@context.LastName" />
                <Column TData="DateTime" Title="วันเกิด" @bind-Field="@context.BirthDate" Format="dd/MM/yyyy" />
                <Column TData="string" Title="จังหวัด" @bind-Field="@context.Province" />
                <ActionColumn Title="จัดการ" Width="160">
                    <Space>
                        <SpaceItem>
                            <Button Size="@ComponentSize.Small" Type="@ButtonType.Link"
                                    OnClick="() => ViewDetail(context.Id)">ดู</Button>
                        </SpaceItem>
                        <SpaceItem>
                            <Button Size="@ComponentSize.Small" Type="@ButtonType.Link"
                                    OnClick="() => Edit(context.Id)">แก้ไข</Button>
                        </SpaceItem>
                    </Space>
                </ActionColumn>
            </Table>
        }
    </SpaceItem>
</Space>

@code {
    private string _keyword = "";
    private int _page = 1;
    private int _size = 10;
    private bool _loading;
    private IPagedResult<CitizenSearchItem>? _result;

    protected override async Task OnInitializedAsync()
    {
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        _page = 1;
        await LoadDataAsync();
    }

    private async Task HandleTableChangeAsync(AntDesign.Table.QueryModel<CitizenSearchItem> query)
    {
        _page = query.PageIndex;
        _size = query.PageSize;
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _loading = true;
        try
        {
            var request = new SearchCitizens.Request(
                Keyword: string.IsNullOrWhiteSpace(_keyword) ? null : _keyword,
                Page: _page,
                Size: _size);

            var result = await Mediator.Send(request);
            if (result.IsSuccess)
                _result = result.Data;
        }
        finally
        {
            _loading = false;
        }
    }

    private void ViewDetail(Guid id) => Nav.NavigateTo($"/citizens/{id}");
    private void Edit(Guid id) => Nav.NavigateTo($"/citizens/{id}/edit");
}
```

- [ ] **Step 4: Build to verify**

Run:
```bash
dotnet build "src/WebUi/WebUi.csproj" --nologo
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Run test to verify it passes**

Run:
```bash
dotnet test "tests/WebUi.Tests\WebUi.Tests.csproj" --filter "ListCitizensTests" --nologo
```

Expected: PASS — 1 test.

- [ ] **Step 6: Commit**

```bash
git add src/WebUi/Components/Pages/ListCitizens.razor tests/WebUi.Tests/Pages/ListCitizensTests.cs
git commit -m "feat: add ListCitizens page with search and paged table"
```

---

## Task 6: Create CitizenDetail.razor (read-only view)

**Files:**
- Create: `src/WebUi/Components/Pages/CitizenDetail.razor`
- Test: `tests/WebUi.Tests/Pages/CitizenDetailTests.cs`

- [ ] **Step 1: Write failing test**

Create `tests/WebUi.Tests/Pages/CitizenDetailTests.cs`:

```csharp
using Application.Commons.Wrappers;
using Application.Features.Citizens.Queries.GetCitizen;
using Mediator;
using System.Net;

namespace WebUi.Tests.Pages;

public class CitizenDetailTests
{
    [Fact]
    public void Renders_CitizenData_WhenFound()
    {
        using var ctx = new TestContext();
        ctx.Services.AddAntDesign();
        var mediator = Substitute.For<IMediator>();
        var detail = new GetCitizen.CitizenDetail
        {
            Id = Guid.NewGuid(),
            IdCardNumber = "1234567890123",
            FirstName = "สมชาย",
            LastName = "ใจดี",
            BirthDate = new DateTime(1990, 1, 1),
            AddressLine1 = "123 ถนนสุขุมวิท",
            SubDistrict = "คลองเตย",
            District = "คลองเตย",
            Province = "กรุงเทพมหานคร",
            PostalCode =10110
        };
        mediator.Send(Arg.Any<GetCitizen.Request>())
                .Returns(ValueTask.FromResult(Result<GetCitizen.CitizenDetail>.Success(detail)));
        ctx.Services.AddScoped(_ => mediator);

        var cut = ctx.RenderComponent<CitizenDetail>(parameters => parameters
            .Add(p => p.Id, detail.Id));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("สมชาย", cut.Markup);
            Assert.Contains("1234567890123", cut.Markup);
            Assert.Contains("กรุงเทพมหานคร", cut.Markup);
        });
    }

    [Fact]
    public void Renders_NotFoundMessage_WhenCitizenMissing()
    {
        using var ctx = new TestContext();
        ctx.Services.AddAntDesign();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetCitizen.Request>())
                .Returns(ValueTask.FromResult(Result<GetCitizen.CitizenDetail>.Failure("ไม่พบข้อมูลลูกค้า", HttpStatusCode.NotFound)));
        ctx.Services.AddScoped(_ => mediator);

        var cut = ctx.RenderComponent<CitizenDetail>(parameters => parameters
            .Add(p => p.Id, Guid.NewGuid()));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("ไม่พบข้อมูล", cut.Markup);
        });
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:
```bash
dotnet test "tests/WebUi.Tests\WebUi.Tests.csproj" --filter "CitizenDetailTests" --nologo
```

Expected: FAIL — `CitizenDetail` not found.

- [ ] **Step 3: Create CitizenDetail.razor**

Create `src/WebUi/Components/Pages/CitizenDetail.razor`:

```razor
@page "/citizens/{Id:guid}"
@rendermode InteractiveServer
@using Application.Commons.Wrappers
@using Application.Features.Citizens.Queries.GetCitizen
@using Mediator
@inject IMediator Mediator
@inject NavigationManager Nav

<PageTitle>รายละเอียดพลเมือง</PageTitle>

<h1>รายละเอียดพลเมือง</h1>

@if (_detail is null)
{
    @if (_notFound)
    {
        <Alert Type="@AlertType.Error" Message="ไม่พบข้อมูลพลเมืองที่ค้นหา" ShowIcon />
        <Button Type="@ButtonType.Default" OnClick="BackToList">กลับสู่รายการ</Button>
    }
    else
    {
        <Spin Spinning />
    }
}
else
{
    <Descriptions Title="ข้อมูลพลเมือง" Bordered Column="2">
        <DescriptionsItem Label="เลขบัตรประชาชน">@_detail.IdCardNumber</DescriptionsItem>
        <DescriptionsItem Label="ชื่อ-นามสกุล">@($"{_detail.FirstName} {_detail.LastName}")</DescriptionsItem>
        <DescriptionsItem Label="วันเกิด">@_detail.BirthDate.ToString("dd/MM/yyyy")</DescriptionsItem>
        <DescriptionsItem Label="รหัสไปรษณีย์">@_detail.PostalCode</DescriptionsItem>
        <DescriptionsItem Label="ที่อยู่" Span="2">@_detail.AddressLine1</DescriptionsItem>
        <DescriptionsItem Label="ตำบล/แขวง">@_detail.SubDistrict</DescriptionsItem>
        <DescriptionsItem Label="อำเภอ/เขต">@_detail.District</DescriptionsItem>
        <DescriptionsItem Label="จังหวัด">@_detail.Province</DescriptionsItem>
        @if (_detail.IdCardImage is not null)
        {
            <DescriptionsItem Label="รูปบัตรประชาชน" Span="2">
                <Image Src="@_detail.IdCardImage" Width="300" />
            </DescriptionsItem>
        }
    </Descriptions>

    <Space Style="margin-top: 16px">
        <SpaceItem>
            <Button Type="@ButtonType.Primary" OnClick="Edit">แก้ไข</Button>
        </SpaceItem>
        <SpaceItem>
            <Button Type="@ButtonType.Default" OnClick="BackToList">กลับสู่รายการ</Button>
        </SpaceItem>
    </Space>
}

@code {
    [Parameter] public Guid Id { get; set; }

    private GetCitizen.CitizenDetail? _detail;
    private bool _notFound;

    protected override async Task OnInitializedAsync()
    {
        var result = await Mediator.Send(new GetCitizen.Request(Id));
        if (result.IsSuccess && result.Data is not null)
            _detail = result.Data;
        else
            _notFound = true;
    }

    private void Edit() => Nav.NavigateTo($"/citizens/{Id}/edit");
    private void BackToList() => Nav.NavigateTo("/citizens");
}
```

- [ ] **Step 4: Build to verify**

Run:
```bash
dotnet build "src/WebUi/WebUi.csproj" --nologo
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Run test to verify it passes**

Run:
```bash
dotnet test "tests/WebUi.Tests\WebUi.Tests.csproj" --filter "CitizenDetailTests" --nologo
```

Expected: PASS — 2 tests.

- [ ] **Step 6: Commit**

```bash
git add src/WebUi/Components/Pages/CitizenDetail.razor tests/WebUi.Tests/Pages/CitizenDetailTests.cs
git commit -m "feat: add CitizenDetail page with read-only Descriptions view"
```

---

## Task 7: Create EditCitizen.razor (load → CitizenForm → UpdateCitizen)

**Files:**
- Create: `src/WebUi/Components/Pages/EditCitizen.razor`
- Test: `tests/WebUi.Tests/Pages/EditCitizenTests.cs`

- [ ] **Step 1: Write failing test**

Create `tests/WebUi.Tests/Pages/EditCitizenTests.cs`:

```csharp
using Application.Commons.Wrappers;
using Application.Features.Citizens.Queries.GetCitizen;
using Application.Features.Locations.Queries.GetAllProvinces;
using Mediator;

namespace WebUi.Tests.Pages;

public class EditCitizenTests
{
    [Fact]
    public void Renders_Form_WhenCitizenFound()
    {
        using var ctx = new TestContext();
        ctx.Services.AddAntDesign();
        var mediator = Substitute.For<IMediator>();
        var detail = new GetCitizen.CitizenDetail
        {
            Id = Guid.NewGuid(),
            IdCardNumber = "1234567890123",
            FirstName = "สมหญิง",
            LastName = "รักไทย",
            BirthDate = new DateTime(1995, 5, 15),
            AddressLine1 = "55 หมู่ 3",
            SubDistrict = "บางรัก",
            District = "บางรัก",
            Province = "กรุงเทพมหานคร",
            PostalCode = "10500"
        };
        mediator.Send(Arg.Any<GetCitizen.Request>())
                .Returns(ValueTask.FromResult(Result<GetCitizen.CitizenDetail>.Success(detail)));
        mediator.Send(Arg.Any<GetAllProvinces.Request>())
                .Returns(ValueTask.FromResult(Result<List<GetAllProvinces.Response>>.Success(new())));
        ctx.Services.AddScoped(_ => mediator);

        var cut = ctx.RenderComponent<EditCitizen>(parameters => parameters
            .Add(p => p.Id, detail.Id));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("แก้ไขข้อมูลพลเมือง", cut.Markup);
            Assert.Contains("สมหญิง", cut.Markup);
        });
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:
```bash
dotnet test "tests/WebUi.Tests\WebUi.Tests.csproj" --filter "EditCitizenTests" --nologo
```

Expected: FAIL — `EditCitizen` not found.

- [ ] **Step 3: Create EditCitizen.razor**

Create `src/WebUi/Components/Pages/EditCitizen.razor`:

```razor
@page "/citizens/{Id:guid}/edit"
@rendermode InteractiveServer
@using Application.Commons.Wrappers
@using Application.Features.Citizens.Commands.UpdateCitizen
@using Application.Features.Citizens.Queries.GetCitizen
@using Mediator
@inject IMediator Mediator
@inject MessageService Message
@inject NavigationManager Nav

<PageTitle>แก้ไขข้อมูลพลเมือง</PageTitle>

<h1>แก้ไขข้อมูลพลเมือง</h1>

@if (_model is null)
{
    @if (_notFound)
    {
        <Alert Type="@AlertType.Error" Message="ไม่พบข้อมูลพลเมืองที่ต้องการแก้ไข" ShowIcon />
        <Button Type="@ButtonType.Default" OnClick="BackToList">กลับสู่รายการ</Button>
    }
    else
    {
        <Spin Spinning />
    }
}
else
{
    <CitizenForm Model="@_model"
                 OnValidSubmit="HandleSubmitAsync"
                 OnCancel="BackToList"
                 SubmitText="บันทึกการแก้ไข"
                 Loading="@_submitting" />
}

@code {
    [Parameter] public Guid Id { get; set; }

    private CitizenFormModel? _model;
    private bool _notFound;
    private bool _submitting;

    protected override async Task OnInitializedAsync()
    {
        var result = await Mediator.Send(new GetCitizen.Request(Id));
        if (!result.IsSuccess || result.Data is null)
        {
            _notFound = true;
            return;
        }

        var c = result.Data;
        _model = new CitizenFormModel
        {
            IdCardNumber = c.IdCardNumber,
            FirstName = c.FirstName,
            LastName = c.LastName,
            BirthDate = c.BirthDate,
            AddressLine1 = c.AddressLine1,
            PostalCode = c.PostalCode,
            IdCardImage = c.IdCardImage,
            // CitizenForm matches these names to dropdown IDs during its OnInitializedAsync
            InitialProvinceName = c.Province,
            InitialDistrictName = c.District,
            InitialSubDistrictName = c.SubDistrict
        };
    }

    private async Task HandleSubmitAsync(CitizenFormModel form)
    {
        if (form.ProvinceId is null || form.DistrictId is null || form.SubDistrictId is null)
        {
            await Message.ErrorAsync("กรุณาเลือกจังหวัด อำเภอ และตำบลให้ครบ");
            return;
        }

        var request = new UpdateCitizen.Request(
            Id: Id,
            IdCardNumber: form.IdCardNumber!,
            FirstName: form.FirstName!,
            LastName: form.LastName!,
            BirthDate: form.BirthDate!.Value,
            AddressLine1: form.AddressLine1!,
            SubDistrict: form.SubDistrict,
            District: form.District,
            Province: form.Province,
            PostalCode: form.PostalCode!,
            IdCardImage: form.IdCardImage);

        _submitting = true;
        try
        {
            var result = await Mediator.Send(request);
            if (result.IsSuccess)
            {
                await Message.SuccessAsync("แก้ไขข้อมูลพลเมืองเรียบร้อย");
                Nav.NavigateTo($"/citizens/{Id}");
            }
            else
            {
                foreach (var msg in result.Messages)
                    await Message.ErrorAsync(msg);
            }
        }
        finally
        {
            _submitting = false;
        }
    }

    private void BackToList() => Nav.NavigateTo("/citizens");
}
```

- [ ] **Step 4: Build to verify**

Run:
```bash
dotnet build "src/WebUi/WebUi.csproj" --nologo
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Run test to verify it passes**

Run:
```bash
dotnet test "tests/WebUi.Tests\WebUi.Tests.csproj" --filter "EditCitizenTests" --nologo
```

Expected: PASS — 1 test.

- [ ] **Step 6: Commit**

```bash
git add src/WebUi/Components/Pages/EditCitizen.razor tests/WebUi.Tests/Pages/EditCitizenTests.cs
git commit -m "feat: add EditCitizen page using shared CitizenForm with pre-population"
```

---

## Task 8: Add IdCardImage upload to CitizenForm

**Files:**
- Create: `src/WebUi/wwwroot/js/citizen-app.js`
- Modify: `src/WebUi/Components/App.razor` (add script reference)
- Modify: `src/WebUi/Components/Shared/CitizenForm.razor` (add Upload component)
- Modify: `src/WebUi/Components/Shared/CitizenForm.razor.cs` (add upload handler + JS interop)

- [ ] **Step 1: Create JS interop file**

Create `src/WebUi/wwwroot/js/citizen-app.js`:

```javascript
window.citizenApp = {
    // Reads a blob URL (from AntDesign Upload ObjectURL) and returns a base64 data URL.
    readBlobAsDataURL: async function (blobUrl) {
        const response = await fetch(blobUrl);
        const blob = await response.blob();
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onloadend = () => resolve(reader.result);
            reader.onerror = reject;
            reader.readAsDataURL(blob);
        });
    }
};
```

- [ ] **Step 2: Add script reference in App.razor**

In `src/WebUi/Components/App.razor`, add the script reference before the Blazor script. Find the line:

```html
    <script src="_content/AntDesign/js/ant-design-blazor.js"></script>
```

Add after it:

```html
    <script src="@Assets["js/citizen-app.js"]"></script>
```

The full `<body>` section should now look like:

```html
<body>
    <Routes />
    <ReconnectModal />
    <AntContainer />
    <script src="_content/AntDesign/js/ant-design-blazor.js"></script>
    <script src="@Assets["js/citizen-app.js"]"></script>
    <script src="@Assets["_framework/blazor.web.js"]"></script>
</body>
```

- [ ] **Step 3: Add Upload component to CitizenForm.razor**

In `src/WebUi/Components/Shared/CitizenForm.razor`, add the upload FormItem BEFORE the submit button FormItem. Insert this block right after the postal code Row block and before the `@if (Model.IdCardImage is not null)` block:

```razor
    <FormItem Label="รูปบัตรประชาชน" Name="IdCardImage">
        <Upload BeforeUpload="HandleBeforeUploadAsync"
                Accept=".jpg,.jpeg,.png,.bmp"
                MaxCount="1"
                ListType="@UploadListType.PictureCard"
                OnRemove="HandleRemoveImage"
                FileList="@_fileList">
            @if (Model.IdCardImage is null)
            {
                <div style="padding: 8px">
                    <Icon Type="@IconType.Outline.Upload" />
                    <div style="margin-top: 8px">อัปโหลด</div>
                </div>
            }
        </Upload>
    </FormItem>
```

Also add `_fileList` field declaration in the `@code` block of the razor file:

```razor
@code {
    internal sealed class ProvinceItem { /* ... existing ... */ }
    internal sealed class DistrictItem { /* ... existing ... */ }
    internal sealed class SubDistrictItem { /* ... existing ... */ }

    private List<UploadFileItem> _fileList = new();
}
```

- [ ] **Step 4: Add upload handlers to CitizenForm.razor.cs**

Add `IJSRuntime` injection and upload handler methods to `src/WebUi/Components/Shared/CitizenForm.razor.cs`. Add the using and injection:

```csharp
using Microsoft.JSInterop;
```

Add to the class body (after the `[Inject] private IMediator Mediator` line):

```csharp
    [Inject] private IJSRuntime JS { get; set; } = default!;
```

Add these methods (after `HandleCancel`):

```csharp
    private async Task<bool> HandleBeforeUploadAsync(UploadFileItem file)
    {
        // Prevent auto-upload to a server URL — we handle the file client-side.
        // Read the blob URL via JS interop and store as base64 data URL.
        if (!string.IsNullOrEmpty(file.ObjectURL))
        {
            try
            {
                Model.IdCardImage = await JS.InvokeAsync<string>("citizenApp.readBlobAsDataURL", file.ObjectURL);
                _fileList.Clear();
                _fileList.Add(file);
            }
            catch
            {
                // If JS interop fails, leave IdCardImage unchanged
            }
        }
        return false; // Always return false to prevent HTTP upload
    }

    private async Task<bool> HandleRemoveImage(UploadFileItem file)
    {
        Model.IdCardImage = null;
        _fileList.Clear();
        await Task.CompletedTask;
        return true;
    }
```

- [ ] **Step 5: Build to verify**

Run:
```bash
dotnet build "src/WebUi/WebUi.csproj" --nologo
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 6: Run all tests to verify nothing broke**

Run:
```bash
dotnet test "tests/WebUi.Tests/WebUi.Tests.csproj" --nologo
```

Expected: All tests PASS.

- [ ] **Step 7: Commit**

```bash
git add src/WebUi/wwwroot/js/citizen-app.js src/WebUi/Components/App.razor src/WebUi/Components/Shared/CitizenForm.razor src/WebUi/Components/Shared/CitizenForm.razor.cs
git commit -m "feat: add ID card image upload to CitizenForm via AntDesign Upload + JS interop"
```

---

## Task 9: Wire navigation (NavMenu + verify cross-page links)

**Files:**
- Modify: `src/WebUi/Components/Layout/NavMenu.razor`

- [ ] **Step 1: Update NavMenu — add citizen list link**

In `src/WebUi/Components/Layout/NavMenu.razor`, add a "รายชื่อพลเมือง" nav item. The file currently has Home and เพิ่มพลเมือง links. Add the list link between Home and เพิ่มพลเมือง:

```razor
<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="">Citizen</a>
    </div>
</div>

<input type="checkbox" title="Navigation menu" class="navbar-toggler" />

<div class="nav-scrollable" onclick="document.querySelector('.navbar-toggler').click()">
    <nav class="nav flex-column">
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span> Home
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="citizens">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> รายชื่อพลเมือง
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="add-citizen">
                <span class="bi bi-plus-square-fill-nav-menu" aria-hidden="true"></span> เพิ่มพลเมือง
            </NavLink>
        </div>
    </nav>
</div>
```

- [ ] **Step 2: Build to verify**

Run:
```bash
dotnet build "src/WebUi/WebUi.csproj" --nologo
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Run all tests**

Run:
```bash
dotnet test "tests/WebUi.Tests\WebUi.Tests.csproj" --nologo
```

Expected: All tests PASS.

- [ ] **Step 4: Manual smoke test — run the app**

Run:
```bash
dotnet run --project "src/WebUi/WebUi.csproj" --no-build
```

Open browser to the displayed URL (typically `http://localhost:5xxx`). Verify:
1. Nav menu shows "รายชื่อพลเมือง" and "เพิ่มพลเมือง"
2. `/citizens` loads the list page with search and table
3. `/add-citizen` loads the form with cascading dropdowns
4. Create a citizen, then click "ดู" → detail page shows data
5. Click "แก้ไข" → edit form pre-populates with province/district/subdistrict selected
6. Upload an image in edit form → image preview appears
7. Save → redirects back to detail page with image shown

- [ ] **Step 5: Commit**

```bash
git add src/WebUi/Components/Layout/NavMenu.razor
git commit -m "feat: wire navigation — add citizen list link to NavMenu"
```

---

## Self-Review

**Spec coverage:**
- ✅ ListCitizens / SearchCitizens page → Task 5
- ✅ EditCitizen page → Task 7
- ✅ Citizen detail page → Task 6
- ✅ IdCardImage upload → Task 8
- ✅ Shared form DRY (Add + Edit) → Tasks 2-4, 7
- ✅ Navigation wiring → Task 9
- ✅ Test infrastructure → Task 1

**Placeholder scan:** No TBD/TODO/"implement later" found. All code blocks contain complete, runnable code.

**Type consistency check:**
- `CitizenFormModel` — defined in Task 2, used consistently in Tasks 3, 4, 7, 8
- `CitizenForm` parameters: `Model`, `OnValidSubmit`, `OnCancel`, `SubmitText`, `Loading` — consistent across Tasks 3, 4, 7
- `ProvinceItem`/`DistrictItem`/`SubDistrictItem` — defined in CitizenForm.razor `@code`, used in CitizenForm.razor.cs — consistent
- `GetAllProvinces.Request` / `GetDistrictsByProvince.Request` / `GetSubDistrictsByDistrict.Request` — fully-qualified names match existing Application layer
- `SearchCitizens.Request(string?, int, int)` — matches Task 5 usage
- `GetCitizen.Request(Guid)` → `Result<CitizenDetail>` — matches Task 6 usage
- `UpdateCitizen.Request(Guid, ...)` — matches Task 7 usage
- `UploadFileItem.ObjectURL` — matches AntDesign source (Task 8)
- `_fileList` field — declared in CitizenForm.razor `@code`, used in CitizenForm.razor.cs — both in same partial class

# Rename Citizen → Customer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rename the entire `Citizen` concept to `Customer` across all layers (Domain, Infrastructure, Application, WebUi), the database schema, and the solution file — because the stored data represents customers, not citizens.

**Architecture:** Pure rename refactor — no behavior change. Bottom-up: SQL script (written but not run) → Domain entity/repository → Infrastructure context/config/repo → Application features (folder + namespace + DTO rename) → WebUi pages/routes/UI text → Solution file rename → Run SQL → Build → Smoke test. Intermediate steps within the rename wave will NOT compile until all layers are done; the build gate is at the end of the wave.

**Tech Stack:** .NET 10, Blazor Server, AntDesign Blazor, Mediator (source-generated), FluentValidation, EF Core, SQL Server — no test project exists (verification = `dotnet build` + manual smoke test)

---

## Rename Map

| Concept | Old | New |
|---------|-----|-----|
| Entity class | `Citizen` | `Customer` |
| Repository interface | `ICitizenRepository` | `ICustomerRepository` |
| Repository impl | `CitizenRepository` | `CustomerRepository` |
| DbContext | `CitizenContext` | `CustomerContext` |
| EF Configuration | `CitizenConfiguration` | `CustomerConfiguration` |
| Feature folder | `Features/Citizens/` | `Features/Customers/` |
| Search DTO | `CitizenSearchItem` | `CustomerSearchItem` |
| Detail DTO | `CitizenDetail` | `CustomerDetail` |
| Form model | `CitizenFormModel` | `CustomerFormModel` |
| Page class | `AddCitizen` / `SearchClitizen` | `AddCustomer` / `SearchCustomer` |
| Page route | `/add-citizen` / `/search-citizen` | `/add-customer` / `/search-customer` |
| Page file | `AddCitizen.razor` / `SearchClitizen.razor` | `AddCustomer.razor` / `SearchCustomer.razor` |
| Field | `IdCardNumber` | `NationalId` |
| Repo methods | `ExistsByIdCardAsync` / `GetByIdCardAsync` | `ExistsByNationalIdAsync` / `GetByNationalIdAsync` |
| DB catalog | `Citizen` | `Customer` |
| DB table | `dbo.Citizens` | `dbo.Customers` |
| DB column | `IdCardNumber` | `NationalId` |
| DB PK | `PK__Citizens__3214EC07FC6943F7` | `PK__Customers__3214EC07FC6943F7` |
| DB index | `UX_Citizens_IdCardNumber` | `UX_Customers_NationalId` |
| Solution | `Citizen.slnx` | `Customer.slnx` |
| Thai UI | พลเมือง / เลขบัตรประชาชน | ลูกค้า / เลขบัตรประจำตัว |

> **Note:** `IdCardImage` field is kept as-is (only `IdCardNumber` → `NationalId` was requested).

---

## File Structure

| File | Responsibility | Action |
|------|---------------|--------|
| `docs/sql/rename-citizen-to-customer.sql` | SQL script: rename column, index, table, PK, catalog | Create |
| `src/Domain/Entities/dbo/Citizen.cs` | Customer entity | Rename → `Customer.cs` + edit |
| `src/Domain/Repositories/ICitizenRepository.cs` | Customer repo interface | Rename → `ICustomerRepository.cs` + edit |
| `src/Domain/efpt.config.json` | EF Power Tools config | Modify |
| `src/Infrastructure/contexts/CitizenContext.cs` | DbContext | Rename → `CustomerContext.cs` + edit |
| `src/Infrastructure/contexts/UnitOfWork.cs` | UoW (references context) | Modify |
| `src/Infrastructure/Configurations/CitizenConfiguration.cs` | EF entity config | Rename → `CustomerConfiguration.cs` + edit |
| `src/Infrastructure/Repositories/CitizenRepository.cs` | Repo impl | Rename → `CustomerRepository.cs` + edit |
| `src/Infrastructure/Repositories/ReadOnlyUnitOfWork.cs` | Read-only UoW | Modify |
| `src/Infrastructure/Repositories/ReadOnlyRepository.cs` | Read-only repo | Modify |
| `src/Infrastructure/DependencyInjection.cs` | DI wiring | Modify |
| `src/Application/Features/Customers/Commands/CreateCustomer/*` | Create command (4 files) | Rename folder + edit |
| `src/Application/Features/Customers/Commands/UpdateCustomer/*` | Update command (4 files) | Rename folder + edit |
| `src/Application/Features/Customers/Queries/GetCustomer/*` | Get query (3 files) | Rename folder + edit |
| `src/Application/Features/Customers/Queries/SearchCustomers/*` | Search query (3 files) | Rename folder + edit |
| `src/WebUi/Components/Pages/AddCitizen.razor` | Add page markup | Rename → `AddCustomer.razor` + edit |
| `src/WebUi/Components/Pages/AddCitizen.razor.cs` | Add page code-behind | Rename → `AddCustomer.razor.cs` + edit |
| `src/WebUi/Components/Pages/SearchClitizen.razor` | Search page markup | Rename → `SearchCustomer.razor` + edit |
| `src/WebUi/Components/Pages/SearchClitizen.razor.cs` | Search page code-behind | Rename → `SearchCustomer.razor.cs` + edit |
| `src/WebUi/Components/Layout/NavMenu.razor` | Nav links | Modify |
| `src/WebUi/appsettings.json` | Connection string | Modify |
| `src/WebUi/appsettings.Development.json` | Connection string (dev) | Modify |
| `Citizen.slnx` | Solution file | Rename → `Customer.slnx` |

---

## Task 1: Write the SQL rename script (do NOT run yet)

**Files:**
- Create: `docs/sql/rename-citizen-to-customer.sql`

- [ ] **Step 1: Create the SQL script**

Create `docs/sql/rename-citizen-to-customer.sql`:

```sql
-- ============================================================
-- Rename Citizen → Customer in database
-- Run AFTER code changes are built successfully.
-- STOP the application first — no active connections to the DB.
-- Run in SSMS / sqlcmd connected to the Citizen database.
-- ============================================================

-- 1. Rename column IdCardNumber → NationalId
EXEC sp_rename 'dbo.Citizens.IdCardNumber', 'NationalId', 'COLUMN';
GO

-- 2. Drop and recreate unique index with new name + new column
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Citizens_IdCardNumber' AND object_id = OBJECT_ID('dbo.Citizens'))
    DROP INDEX UX_Citizens_IdCardNumber ON dbo.Citizens;
GO

CREATE UNIQUE INDEX UX_Customers_NationalId ON dbo.Customers(NationalId);
GO

-- 3. Rename table Citizens → Customers
EXEC sp_rename 'dbo.Citizens', 'Customers';
GO

-- 4. Rename primary key constraint
EXEC sp_rename 'PK__Citizens__3214EC07FC6943F7', 'PK__Customers__3214EC07FC6943F7', 'OBJECT';
GO

-- 5. Rename database catalog Citizen → Customer
--    Requires exclusive access — kill active connections first.
--    Uncomment and run separately if connections block it.
/*
ALTER DATABASE Citizen SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
ALTER DATABASE Citizen MODIFY NAME = Customer;
ALTER DATABASE Customer SET MULTI_USER;
GO
*/
```

- [ ] **Step 2: Commit**

```bash
git add docs/sql/rename-citizen-to-customer.sql
git commit -m "chore: add SQL script for Citizen→Customer DB rename"
```

---

## Task 2: Rename Domain layer

> **Warning:** After this task, Infrastructure/Application/WebUi will NOT compile until Tasks 3–5 are also done. Do not build until Task 6.

**Files:**
- Rename: `src/Domain/Entities/dbo/Citizen.cs` → `src/Domain/Entities/dbo/Customer.cs`
- Rename: `src/Domain/Repositories/ICitizenRepository.cs` → `src/Domain/Repositories/ICustomerRepository.cs`
- Modify: `src/Domain/efpt.config.json`

- [ ] **Step 1: Rename and edit the entity file**

Rename `src/Domain/Entities/dbo/Citizen.cs` → `src/Domain/Entities/dbo/Customer.cs`, then replace contents:

```csharp
// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public  class Customer : AuditableEntity
{

    public string NationalId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public string AddressLine1 { get; set; } = null!;

    public string SubDistrict { get; set; } = null!;

    public string District { get; set; } = null!;

    public string Province { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string? IdCardImage { get; set; }
}
```

- [ ] **Step 2: Rename and edit the repository interface**

Rename `src/Domain/Repositories/ICitizenRepository.cs` → `src/Domain/Repositories/ICustomerRepository.cs`, then replace contents:

```csharp
using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<bool> ExistsByNationalIdAsync(string nationalId, Guid? excludingId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 3: Edit efpt.config.json**

In `src/Domain/efpt.config.json`, change line 3 and line 26:

```json
   "ContextClassName": "CustomerContext",
```
```json
      {
         "Name": "[dbo].[Customers]",
         "ObjectType": 0
      }
```

- [ ] **Step 4: Commit**

```bash
git add src/Domain/
git commit -m "refactor: rename Citizen→Customer in Domain layer"
```

---

## Task 3: Rename Infrastructure layer

**Files:**
- Rename: `src/Infrastructure/contexts/CitizenContext.cs` → `src/Infrastructure/contexts/CustomerContext.cs`
- Rename: `src/Infrastructure/Configurations/CitizenConfiguration.cs` → `src/Infrastructure/Configurations/CustomerConfiguration.cs`
- Rename: `src/Infrastructure/Repositories/CitizenRepository.cs` → `src/Infrastructure/Repositories/CustomerRepository.cs`
- Modify: `src/Infrastructure/contexts/UnitOfWork.cs`
- Modify: `src/Infrastructure/Repositories/ReadOnlyUnitOfWork.cs`
- Modify: `src/Infrastructure/Repositories/ReadOnlyRepository.cs`
- Modify: `src/Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: Rename and edit CustomerContext**

Rename `src/Infrastructure/contexts/CitizenContext.cs` → `src/Infrastructure/contexts/CustomerContext.cs`, then replace contents:

```csharp
// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
#nullable enable

namespace Infrastructure.contexts;

public partial class CustomerContext : DbContext
{
    public CustomerContext(DbContextOptions<CustomerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Audit> Audits { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<SubDistrict> SubDistricts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Infrastructure.Configurations.AuditConfiguration());
        modelBuilder.ApplyConfiguration(new Infrastructure.Configurations.CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new Infrastructure.Configurations.ProvinceConfiguration());
        modelBuilder.ApplyConfiguration(new Infrastructure.Configurations.DistrictConfiguration());
        modelBuilder.ApplyConfiguration(new Infrastructure.Configurations.SubDistrictConfiguration());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
```

- [ ] **Step 2: Rename and edit CustomerConfiguration**

Rename `src/Infrastructure/Configurations/CitizenConfiguration.cs` → `src/Infrastructure/Configurations/CustomerConfiguration.cs`, then replace contents:

```csharp
// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

#nullable disable

namespace Infrastructure.Configurations
{
    public partial class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> entity)
        {
            entity.HasKey(e => e.Id).HasName("PK__Customers__3214EC07FC6943F7");

            entity.HasIndex(e => e.NationalId, "UX_Customers_NationalId").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AddressLine1).HasMaxLength(300);
            entity.Property(e => e.BirthDate).HasColumnType("date");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasDefaultValue("System");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.NationalId).HasMaxLength(13);
            entity.Property(e => e.SubDistrict).HasMaxLength(100);
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(10);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<Customer> entity);
    }
}
```

- [ ] **Step 3: Rename and edit CustomerRepository**

Rename `src/Infrastructure/Repositories/CitizenRepository.cs` → `src/Infrastructure/Repositories/CustomerRepository.cs`, then replace contents:

```csharp
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(CustomerContext context) : base(context) { }

    public Task<bool> ExistsByNationalIdAsync(string nationalId, Guid? excludingId, CancellationToken cancellationToken = default)
    {
        return Entities
            .Where(c => c.NationalId == nationalId)
            .Where(c => excludingId == null || c.Id != excludingId)
            .AnyAsync(cancellationToken);
    }

    public Task<Customer?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken = default)
    {
        return Entities.FirstOrDefaultAsync(c => c.NationalId == nationalId, cancellationToken);
    }
}
```

- [ ] **Step 4: Edit UnitOfWork.cs**

In `src/Infrastructure/contexts/UnitOfWork.cs`, replace `CitizenContext` with `CustomerContext` (3 occurrences: field declaration line 11, constructor parameter line 15, constructor body line 17):

```csharp
    private readonly CustomerContext _context;
    private IAuditRepository? _audits;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(CustomerContext context)
    {
        _context = context;
    }
```

- [ ] **Step 5: Edit ReadOnlyUnitOfWork.cs**

In `src/Infrastructure/Repositories/ReadOnlyUnitOfWork.cs`, replace `CitizenContext` with `CustomerContext` (3 occurrences: field line 9, list line 10, factory parameter line 14):

```csharp
    private readonly IDbContextFactory<CustomerContext> _factory;
    private readonly List<CustomerContext> _contexts = new();
    private readonly Dictionary<Type, object> _repositories = new();
    private bool _disposed;

    public ReadOnlyUnitOfWork(IDbContextFactory<CustomerContext> factory)
    {
        _factory = factory;
    }
```

And in the `Repository<T>()` method, line 26:

```csharp
            var context = _factory.CreateDbContext();
```
(This line stays the same — `_factory` is now `IDbContextFactory<CustomerContext>`.)

- [ ] **Step 6: Edit ReadOnlyRepository.cs**

In `src/Infrastructure/Repositories/ReadOnlyRepository.cs`, replace `CitizenContext` with `CustomerContext` (2 occurrences: field line 9, constructor parameter line 11):

```csharp
    private readonly CustomerContext _context;

    public ReadOnlyRepository(CustomerContext context)
    {
        _context = context;
    }
```

- [ ] **Step 7: Edit DependencyInjection.cs**

In `src/Infrastructure/DependencyInjection.cs`, replace all `CitizenContext` with `CustomerContext` (lines 23, 34, 45) and `ICitizenRepository`/`CitizenRepository` with `ICustomerRepository`/`CustomerRepository` (line 47):

```csharp
        services.AddDbContext<CustomerContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        services.AddDbContextFactory<CustomerContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        }, ServiceLifetime.Scoped);

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<CustomerContext>());

        services.AddScoped<ICustomerRepository, CustomerRepository>();
```

- [ ] **Step 8: Commit**

```bash
git add src/Infrastructure/
git commit -m "refactor: rename Citizen→Customer in Infrastructure layer"
```

---

## Task 4: Rename Application layer (feature folders + namespaces + DTOs)

**Files:**
- Rename: `src/Application/Features/Citizens/` → `src/Application/Features/Customers/`
- Rename subfolders: `CreateCitizen`→`CreateCustomer`, `UpdateCitizen`→`UpdateCustomer`, `GetCitizen`→`GetCustomer`, `SearchCitizens`→`SearchCustomers`
- Edit: all 14 files inside (namespaces, class names, field names, Thai messages)

- [ ] **Step 1: Rename the feature folders**

```bash
cd src/Application/Features
mv Citizens Customers
cd Customers/Commands
mv CreateCitizen CreateCustomer
mv UpdateCitizen UpdateCustomer
cd ../Queries
mv GetCitizen GetCustomer
mv SearchCitizens SearchCustomers
```

- [ ] **Step 2: Edit CreateCustomer/Request.cs**

Replace contents of `src/Application/Features/Customers/Commands/CreateCustomer/Request.cs`:

```csharp
using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Customers.Commands.CreateCustomer;

public sealed record Request(
    string NationalId,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string AddressLine1,
    string SubDistrict,
    string District,
    string Province,
    string PostalCode,
    string? IdCardImage = null) : IRequest<Result<Response>>;
```

- [ ] **Step 3: Edit CreateCustomer/Response.cs**

Replace contents of `src/Application/Features/Customers/Commands/CreateCustomer/Response.cs`:

```csharp
namespace Application.Features.Customers.Commands.CreateCustomer;

public sealed record Response(Guid Id);
```

- [ ] **Step 4: Edit CreateCustomer/RequestValidator.cs**

Replace contents of `src/Application/Features/Customers/Commands/CreateCustomer/RequestValidator.cs`:

```csharp
using FluentValidation;

namespace Application.Features.Customers.Commands.CreateCustomer;

public sealed class RequestValidator : AbstractValidator<Request>
{
    public RequestValidator()
    {
        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("กรุณากรอกเลขบัตรประจำตัว")
            .Length(13).WithMessage("เลขบัตรประจำตัวต้องมี 13 หลัก")
            .Matches(@"^\d{13}$").WithMessage("เลขบัตรประจำตัวต้องเป็นตัวเลข 13 หลัก");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("กรุณากรอกชื่อ")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("กรุณากรอกนามสกุล")
            .MaximumLength(100);

        RuleFor(x => x.BirthDate)
            .NotEqual(default(DateTime)).WithMessage("กรุณากรอกวันเกิด")
            .LessThanOrEqualTo((DateTime.Today)).WithMessage("วันเกิดต้องไม่เป็นวันในอนาคต");

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
```

- [ ] **Step 5: Edit CreateCustomer/RequestHandler.cs**

Replace contents of `src/Application/Features/Customers/Commands/CreateCustomer/RequestHandler.cs`:

```csharp
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

    public RequestHandler(IUnitOfWork unitOfWork, ICustomerRepository customerRepository)
    {
        _unitOfWork = unitOfWork;
        _customerRepository = customerRepository;
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
                IdCardImage = request.IdCardImage
            };

            await _unitOfWork.Repository<Customer>().AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Response>.Success(new Response(entity.Id), statusCode: HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return Result<Response>.Failure($"Failed to create customer: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
```

- [ ] **Step 6: Edit UpdateCustomer/Request.cs**

Replace contents of `src/Application/Features/Customers/Commands/UpdateCustomer/Request.cs`:

```csharp
using Application.Commons.Wrappers;
using Mediator;
using System.Text.Json.Serialization;

namespace Application.Features.Customers.Commands.UpdateCustomer;

public sealed record Request(
    [property: JsonIgnore] Guid Id,
    string NationalId,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string AddressLine1,
    string SubDistrict,
    string District,
    string Province,
    string PostalCode,
    string? IdCardImage = null) : IRequest<Result<Response>>;
```

- [ ] **Step 7: Edit UpdateCustomer/Response.cs**

Replace contents of `src/Application/Features/Customers/Commands/UpdateCustomer/Response.cs`:

```csharp
namespace Application.Features.Customers.Commands.UpdateCustomer;

public sealed record Response(Guid Id);
```

- [ ] **Step 8: Edit UpdateCustomer/RequestValidator.cs**

Replace contents of `src/Application/Features/Customers/Commands/UpdateCustomer/RequestValidator.cs`:

```csharp
using FluentValidation;

namespace Application.Features.Customers.Commands.UpdateCustomer;

public sealed class RequestValidator : AbstractValidator<Request>
{
    public RequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("กรุณากรอกเลขบัตรประจำตัว")
            .Length(13).WithMessage("เลขบัตรประจำตัวต้องมี 13 หลัก")
            .Matches(@"^\d{13}$").WithMessage("เลขบัตรประจำตัวต้องเป็นตัวเลข 13 หลัก");

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
```

- [ ] **Step 9: Edit UpdateCustomer/RequestHandler.cs**

Replace contents of `src/Application/Features/Customers/Commands/UpdateCustomer/RequestHandler.cs`:

```csharp
using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Application.Features.Customers.Commands.UpdateCustomer;

public sealed class RequestHandler : IRequestHandler<Request, Result<Response>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerRepository _customerRepository;

    public RequestHandler(IUnitOfWork unitOfWork, ICustomerRepository customerRepository)
    {
        _unitOfWork = unitOfWork;
        _customerRepository = customerRepository;
    }

    public async ValueTask<Result<Response>> Handle(Request request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _unitOfWork.Repository<Customer>().Entities
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (customer is null)
                return Result<Response>.Failure("ไม่พบข้อมูลลูกค้า", HttpStatusCode.NotFound);

            var duplicate = await _customerRepository.ExistsByNationalIdAsync(request.NationalId, excludingId: request.Id, cancellationToken);
            if (duplicate)
                return Result<Response>.Failure("เลขบัตรประจำตัวนี้มีอยู่แล้วในระบบ", HttpStatusCode.BadRequest);

            customer.NationalId = request.NationalId;
            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.BirthDate = request.BirthDate;
            customer.AddressLine1 = request.AddressLine1;
            customer.SubDistrict = request.SubDistrict;
            customer.District = request.District;
            customer.Province = request.Province;
            customer.PostalCode = request.PostalCode;
            customer.IdCardImage = request.IdCardImage;
            customer.UpdatedDate = DateTime.UtcNow;
            customer.UpdatedBy = "System";

            _unitOfWork.Repository<Customer>().Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Response>.Success(new Response(customer.Id));
        }
        catch (Exception ex)
        {
            return Result<Response>.Failure($"Failed to update customer: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
```

- [ ] **Step 10: Edit GetCustomer/Request.cs**

Replace contents of `src/Application/Features/Customers/Queries/GetCustomer/Request.cs`:

```csharp
using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Customers.Queries.GetCustomer;

public sealed record Request(Guid Id) : IRequest<Result<CustomerDetail>>;
```

- [ ] **Step 11: Edit GetCustomer/Response.cs**

Replace contents of `src/Application/Features/Customers/Queries/GetCustomer/Response.cs`:

```csharp
namespace Application.Features.Customers.Queries.GetCustomer;

public sealed class CustomerDetail
{
    public Guid Id { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string SubDistrict { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? IdCardImage { get; set; }
}
```

- [ ] **Step 12: Edit GetCustomer/RequestHandler.cs**

Replace contents of `src/Application/Features/Customers/Queries/GetCustomer/RequestHandler.cs`:

```csharp
using Application.Commons.Wrappers;
using Domain.Common;
using Domain.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Application.Features.Customers.Queries.GetCustomer;

public sealed class RequestHandler : IRequestHandler<Request, Result<CustomerDetail>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<CustomerDetail>> Handle(Request request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Repository<Customer>().Entities
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer is null)
            return Result<CustomerDetail>.Failure("ไม่พบข้อมูลลูกค้า", HttpStatusCode.NotFound);

        return Result<CustomerDetail>.Success(new CustomerDetail
        {
            Id = customer.Id,
            NationalId = customer.NationalId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            BirthDate = customer.BirthDate,
            AddressLine1 = customer.AddressLine1,
            SubDistrict = customer.SubDistrict,
            District = customer.District,
            Province = customer.Province,
            PostalCode = customer.PostalCode,
            IdCardImage = customer.IdCardImage
        });
    }
}
```

- [ ] **Step 13: Edit SearchCustomers/Request.cs**

Replace contents of `src/Application/Features/Customers/Queries/SearchCustomers/Request.cs`:

```csharp
using Application.Commons.Wrappers;
using Mediator;

namespace Application.Features.Customers.Queries.SearchCustomers;

public sealed record Request(
    string? NationalId,
    string? FirstName,
    string? LastName,
    string? Province,
    string? District,
    string? SubDistrict,
    string? PostalCode,
    int Page = 1,
    int Size = 10) : IRequest<Result<IPagedResult<CustomerSearchItem>>>;
```

- [ ] **Step 14: Edit SearchCustomers/Response.cs**

Replace contents of `src/Application/Features/Customers/Queries/SearchCustomers/Response.cs`:

```csharp
namespace Application.Features.Customers.Queries.SearchCustomers;

public sealed class CustomerSearchItem
{
    public Guid Id { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string SubDistrict { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}
```

- [ ] **Step 15: Edit SearchCustomers/RequestHandler.cs**

Replace contents of `src/Application/Features/Customers/Queries/SearchCustomers/RequestHandler.cs`:

```csharp
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
```

- [ ] **Step 16: Commit**

```bash
git add src/Application/
git commit -m "refactor: rename Citizen→Customer in Application layer"
```

---

## Task 5: Rename WebUi layer

**Files:**
- Rename: `src/WebUi/Components/Pages/AddCitizen.razor` → `src/WebUi/Components/Pages/AddCustomer.razor`
- Rename: `src/WebUi/Components/Pages/AddCitizen.razor.cs` → `src/WebUi/Components/Pages/AddCustomer.razor.cs`
- Rename: `src/WebUi/Components/Pages/SearchClitizen.razor` → `src/WebUi/Components/Pages/SearchCustomer.razor`
- Rename: `src/WebUi/Components/Pages/SearchClitizen.razor.cs` → `src/WebUi/Components/Pages/SearchCustomer.razor.cs`
- Modify: `src/WebUi/Components/Layout/NavMenu.razor`
- Modify: `src/WebUi/appsettings.json`
- Modify: `src/WebUi/appsettings.Development.json`

- [ ] **Step 1: Rename and edit AddCustomer.razor**

Rename `src/WebUi/Components/Pages/AddCitizen.razor` → `src/WebUi/Components/Pages/AddCustomer.razor`, then replace contents:

```razor
@page "/add-customer"
@rendermode InteractiveServer
@using Application.Commons.Wrappers
@using System.ComponentModel.DataAnnotations

<PageTitle>เพิ่มลูกค้า</PageTitle>

<h1>เพิ่มลูกค้า</h1>

<Form Model="@model" Layout="@FormLayout.Vertical" OnFinish="HandleSubmitAsync" OnFinishFailed="OnSubmitFailedAsync" ValidateOnChange="@validateOnChange">
    <FormItem Label="เลขบัตรประจำตัว" Name="NationalId">
        <Input @bind-Value="@context.NationalId" Placeholder="กรอกเลขบัตรประจำตัว 13 หลัก" MaxLength="13" />
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
                        DataSource="@provinces"
                        ItemLabel="p => p.ProvinceThai"
                        ItemValue="p => (int?)p.ProvinceID"
                        @bind-Value="@context.ProvinceId"
                        Placeholder="เลือกจังหวัด"
                        OnSelectedItemChanged="OnProvinceChangedAsync"
                        AllowClear="true"
                        Loading="@provincesLoading"
                        Disabled="@provincesLoading"
                        Style="width: 100%" />
            </FormItem>
        </Col>
        <Col Xs="24" Sm="12">
            <FormItem Label="อำเภอ/เขต" Name="DistrictId">
                <Select TItem="DistrictItem" TItemValue="int?"
                        DataSource="@districts"
                        ItemLabel="d => d.DistrictThai"
                        ItemValue="d => (int?)d.DistrictID"
                        @bind-Value="@context.DistrictId"
                        Placeholder="เลือกอำเภอ/เขต"
                        OnSelectedItemChanged="OnDistrictChangedAsync"
                        Disabled="@(context.ProvinceId is null)"
                        AllowClear="true"
                        Style="width: 100%" />
            </FormItem>
        </Col>
    </Row>

    <Row Gutter="(16, 16)">
        <Col Xs="24" Sm="12">
            <FormItem Label="ตำบล/แขวง" Name="SubDistrictId">
                <Select TItem="SubDistrictItem" TItemValue="int?"
                        DataSource="@subDistricts"
                        ItemLabel="s => s.TambonThai"
                        ItemValue="s => (int?)s.TambonID"
                        @bind-Value="@context.SubDistrictId"
                        Placeholder="เลือกตำบล/แขวง"
                        OnSelectedItemChanged="OnSubDistrictChanged"
                        Disabled="@(context.DistrictId is null)"
                        AllowClear="true"
                        Style="width: 100%" />
            </FormItem>
        </Col>
        <Col Xs="24" Sm="12">
            <FormItem Label="รหัสไปรษณีย์" Name="PostalCode" >
                <Input @bind-Value="@context.PostalCode" Placeholder="รหัสไปรษณีย์ 5 หลัก" MaxLength="5" ReadOnly />
            </FormItem>
        </Col>
    </Row>

    <FormItem>
        <Space>
            <SpaceItem>
                <Button Type="@ButtonType.Primary" HtmlType="submit" Loading="@submitting">
                    บันทึก
                </Button>
            </SpaceItem>
            <SpaceItem>
                <Button Type="@ButtonType.Default" OnClick="ResetForm">ล้างฟอร์ม</Button>
            </SpaceItem>
        </Space>
    </FormItem>
</Form>
```

- [ ] **Step 2: Rename and edit AddCustomer.razor.cs**

Rename `src/WebUi/Components/Pages/AddCitizen.razor.cs` → `src/WebUi/Components/Pages/AddCustomer.razor.cs`, then replace contents:

```csharp
using AntDesign;
using Application.Commons.Wrappers;
using Mediator;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using WebUi.Services;

namespace WebUi.Components.Pages;

public partial class AddCustomer
{
    [Inject] private ScopedMediator Mediator { get; set; } = default!;
    [Inject] private MessageService Message { get; set; } = default!;
    [Inject] private WebUi.Services.ErrorDialogService ErrorDialog { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private CustomerFormModel model = new();
    private bool submitting;
    private bool validateOnChange; // Enable live re-validation only after first submit attempt

    private List<ProvinceItem> provinces = new();
    private List<DistrictItem> districts = new();
    private List<SubDistrictItem> subDistricts = new();
    private bool provincesLoading = true;

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
            IdCardImage: null);

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
```

- [ ] **Step 3: Rename and edit SearchCustomer.razor**

Rename `src/WebUi/Components/Pages/SearchClitizen.razor` → `src/WebUi/Components/Pages/SearchCustomer.razor`, then replace contents:

```razor
@page "/search-customer"
@rendermode InteractiveServer
@using Application.Commons.Wrappers
@using Application.Features.Customers.Queries.SearchCustomers

<PageTitle>ค้นหาลูกค้า</PageTitle>

<h1>ค้นหาลูกค้า</h1>

<Row Gutter="(16, 16)" Style="margin-bottom: 16px;">
    <Col Xs="24" Sm="12" Md="8">
        <Input @bind-Value="@nationalId" Placeholder="เลขบัตรประจำตัว"
               AllowClear="true" Style="width: 100%" />
    </Col>
    <Col Xs="24" Sm="12" Md="8">
        <Input @bind-Value="@firstName" Placeholder="ชื่อ"
               AllowClear="true" Style="width: 100%" />
    </Col>
    <Col Xs="24" Sm="12" Md="8">
        <Input @bind-Value="@lastName" Placeholder="นามสกุล"
               AllowClear="true" Style="width: 100%" />
    </Col>
</Row>

<Row Gutter="(16, 16)" Style="margin-bottom: 16px;">
    <Col Xs="24" Sm="12" Md="8">
        <Select TItem="ProvinceItem" TItemValue="int?"
                DataSource="@provinces"
                ItemLabel="p => p.ProvinceThai"
                ItemValue="p => (int?)p.ProvinceID"
                @bind-Value="@selectedProvinceId"
                Placeholder="จังหวัด"
                OnSelectedItemChanged="OnProvinceChangedAsync"
                AllowClear="true"
                Loading="@provincesLoading"
                Disabled="@provincesLoading"
                Style="width: 100%" />
    </Col>
    <Col Xs="24" Sm="12" Md="8">
        <Select TItem="DistrictItem" TItemValue="int?"
                DataSource="@districts"
                ItemLabel="d => d.DistrictThai"
                ItemValue="d => (int?)d.DistrictID"
                @bind-Value="@selectedDistrictId"
                Placeholder="อำเภอ/เขต"
                OnSelectedItemChanged="OnDistrictChangedAsync"
                Disabled="@(selectedProvinceId is null)"
                AllowClear="true"
                Style="width: 100%" />
    </Col>
    <Col Xs="24" Sm="12" Md="8">
        <Select TItem="SubDistrictItem" TItemValue="int?"
                DataSource="@subDistricts"
                ItemLabel="s => s.TambonThai"
                ItemValue="s => (int?)s.TambonID"
                @bind-Value="@selectedSubDistrictId"
                Placeholder="ตำบล/แขวง"
                OnSelectedItemChanged="OnSubDistrictChanged"
                Disabled="@(selectedDistrictId is null)"
                AllowClear="true"
                Style="width: 100%" />
    </Col>
</Row>

<Row Gutter="(16, 16)" Style="margin-bottom: 16px;">
    <Col Xs="24" Sm="12" Md="8">
        <Input @bind-Value="@postalCode" Placeholder="รหัสไปรษณีย์"
               AllowClear="true" MaxLength="5"
               Disabled="@(selectedSubDistrictId is not null)"
               Style="width: 100%" />
    </Col>
    <Col Xs="24" Sm="24" Md="16">
        <Space>
            <SpaceItem>
                <Button Type="@ButtonType.Primary" Loading="@loading" OnClick="OnSearchClickAsync">
                    ค้นหา
                </Button>
            </SpaceItem>
            <SpaceItem>
                <Button Type="@ButtonType.Default" OnClick="ResetFilters">
                    ล้าง
                </Button>
            </SpaceItem>
        </Space>
    </Col>
</Row>

<Table TItem="CustomerSearchItem"
       DataSource="@rows"
       Total="@total"
       Loading="@loading"
       PageIndex="@pageIndex"
       PageSize="@pageSize"
       OnChange="OnTableChangeAsync"
       RemoteDataSource="true"
       Size="TableSize.Middle"
       ScrollX="960"
       RowKey="c => c.Id.ToString()">
    <PropertyColumn Property="c => c.NationalId" Title="เลขบัตรประจำตัว" Width="160" />
    <Column TData="string" Title="ชื่อ-นามสกุล" Width="180">
        @(context.FirstName) @(context.LastName)
    </Column>
    <Column TData="string" Title="วันเกิด" Width="120">
        @context.BirthDate.ToString("dd/MM/yyyy")
    </Column>
    <PropertyColumn Property="c => c.AddressLine1" Title="ที่อยู่" Ellipsis Width="220" />
    <PropertyColumn Property="c => c.Province" Title="จังหวัด" Width="120" />
    <PropertyColumn Property="c => c.PostalCode" Title="รหัสไปรษณีย์" Width="110" />
    <ActionColumn Title="จัดการ" Width="120" Fixed="ColumnFixPlacement.Right">
        <Button Type="@ButtonType.Link" Size="ButtonSize.Small" OnClick="@(() => OpenDetailAsync(context.Id))">
            ดูรายละเอียด
        </Button>
    </ActionColumn>
</Table>

<Drawer Title="@("รายละเอียดลูกค้า")"
        Width="480"
        Visible="@detailVisible"
        OnClose="OnDetailClose">
    @if (detailLoading)
    {
        <div style="text-align: center; padding: 48px 0;">
            <Spin />
        </div>
    }
    else if (detail is not null)
    {
        <Descriptions Bordered Size="DescriptionsSize.Small" Column="1">
            <DescriptionsItem Title="เลขบัตรประจำตัว">@detail.NationalId</DescriptionsItem>
            <DescriptionsItem Title="ชื่อ">@detail.FirstName</DescriptionsItem>
            <DescriptionsItem Title="นามสกุล">@detail.LastName</DescriptionsItem>
            <DescriptionsItem Title="วันเกิด">@detail.BirthDate.ToString("dd/MM/yyyy")</DescriptionsItem>
            <DescriptionsItem Title="ที่อยู่">@detail.AddressLine1</DescriptionsItem>
            <DescriptionsItem Title="ตำบล/แขวง">@detail.SubDistrict</DescriptionsItem>
            <DescriptionsItem Title="อำเภอ/เขต">@detail.District</DescriptionsItem>
            <DescriptionsItem Title="จังหวัด">@detail.Province</DescriptionsItem>
            <DescriptionsItem Title="รหัสไปรษณีย์">@detail.PostalCode</DescriptionsItem>
        </Descriptions>
    }
    else
    {
        <Empty Description="@("ไม่พบข้อมูล")" />
    }
</Drawer>
```

- [ ] **Step 4: Rename and edit SearchCustomer.razor.cs**

Rename `src/WebUi/Components/Pages/SearchClitizen.razor.cs` → `src/WebUi/Components/Pages/SearchCustomer.razor.cs`, then replace contents:

```csharp
using AntDesign;
using AntDesign.TableModels;
using Application.Commons.Wrappers;
using Application.Features.Customers.Queries.GetCustomer;
using Application.Features.Customers.Queries.SearchCustomers;
using Mediator;
using Microsoft.AspNetCore.Components;
using WebUi.Services;

using SearchRequest = Application.Features.Customers.Queries.SearchCustomers.Request;
using DetailRequest = Application.Features.Customers.Queries.GetCustomer.Request;

namespace WebUi.Components.Pages;

public partial class SearchCustomer
{
    [Inject] private ScopedMediator Mediator { get; set; } = default!;
    [Inject] private MessageService Message { get; set; } = default!;
    [Inject] private WebUi.Services.ErrorDialogService ErrorDialog { get; set; } = default!;

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
        await InvokeAsync(StateHasChanged);

        try
        {
            var result = await Mediator.Send(new DetailRequest(id));
            if (result.IsSuccess)
                detail = result.Data;
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
```

- [ ] **Step 5: Edit NavMenu.razor**

In `src/WebUi/Components/Layout/NavMenu.razor`, replace the two NavLink blocks (lines 17–27):

```razor
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="add-customer">
                <span class="bi bi-plus-square-fill-nav-menu" aria-hidden="true"></span> เพิ่มลูกค้า
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="search-customer">
                <span class="bi bi-search-nav-menu" aria-hidden="true"></span> ค้นหาลูกค้า
            </NavLink>
        </div>
```

- [ ] **Step 6: Edit appsettings.json**

In `src/WebUi/appsettings.json`, change `Initial Catalog=Citizen` to `Initial Catalog=Customer` on line 10:

```json
    "DefaultConnection": "Data Source=localhost;Initial Catalog=Customer;Persist Security Info=True;User ID=sa;Password=Permsri@1;Encrypt=False;Trust Server Certificate=True"
```

- [ ] **Step 7: Edit appsettings.Development.json**

In `src/WebUi/appsettings.Development.json`, change `Initial Catalog=Citizen` to `Initial Catalog=Customer` on line 9:

```json
    "DefaultConnection": "Data Source=localhost;Initial Catalog=Customer;Persist Security Info=True;User ID=sa;Password=Permsri@1;Encrypt=False;Trust Server Certificate=True"
```

- [ ] **Step 8: Commit**

```bash
git add src/WebUi/
git commit -m "refactor: rename Citizen→Customer in WebUi layer"
```

---

## Task 6: Build verification (gate — must pass before proceeding)

- [ ] **Step 1: Build the solution**

Run:
```bash
dotnet build Citizen.slnx
```
Expected: Build succeeded, 0 errors.

If errors appear, they will be missed references to old names. Search and fix:
```bash
rg "Citizen|IdCardNumber" src/ --glob "*.cs" --glob "*.razor"
```
Fix any remaining references, then rebuild.

- [ ] **Step 2: Clean up stale files**

If old `.razor` / `.cs` files remain after rename (Windows `mv` sometimes leaves copies), delete them:
```bash
ls src/WebUi/Components/Pages/
```
Verify only `AddCustomer.razor`, `AddCustomer.razor.cs`, `SearchCustomer.razor`, `SearchCustomer.razor.cs` exist (no `AddCitizen.*` or `SearchClitizen.*`).

If stale files exist, remove them:
```bash
rm src/WebUi/Components/Pages/AddCitizen.razor
rm src/WebUi/Components/Pages/AddCitizen.razor.cs
rm src/WebUi/Components/Pages/SearchClitizen.razor
rm src/WebUi/Components/Pages/SearchClitizen.razor.cs
```

- [ ] **Step 3: Commit if any fixes were made**

```bash
git add -A
git commit -m "fix: resolve remaining Citizen references after rename"
```

---

## Task 7: Rename solution file

**Files:**
- Rename: `Citizen.slnx` → `Customer.slnx`

- [ ] **Step 1: Rename the solution file**

```bash
git mv Citizen.slnx Customer.slnx
```

- [ ] **Step 2: Build with new solution name**

```bash
dotnet build Customer.slnx
```
Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "refactor: rename solution Citizen.slnx→Customer.slnx"
```

---

## Task 8: Run SQL script and final smoke test

> **Prerequisite:** All code changes committed and building. Stop the running app if any.

- [ ] **Step 1: Stop the application**

Ensure no process is connected to the `Citizen` database (stop `dotnet run`, close browser tabs, etc.).

- [ ] **Step 2: Run the SQL script**

Open SSMS (or `sqlcmd`) connected to the `Citizen` database and run the script from `docs/sql/rename-citizen-to-customer.sql` step-by-step:

1. Rename column `IdCardNumber` → `NationalId`
2. Drop + recreate index → `UX_Customers_NationalId`
3. Rename table `Citizens` → `Customers`
4. Rename PK → `PK__Customers__3214EC07FC6943F7`
5. Rename database `Citizen` → `Customer` (uncomment the `ALTER DATABASE` block — run in `master` context)

Verify:
```sql
USE Customer;
SELECT TOP 1 * FROM dbo.Customers;
SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Customers');
```
Expected: table `dbo.Customers` exists, column `NationalId` present, indexes `PK__Customers__...` and `UX_Customers_NationalId` exist.

- [ ] **Step 3: Build and run the app**

```bash
dotnet build Customer.slnx
cd src/WebUi
dotnet run
```

- [ ] **Step 4: Smoke test — Add customer**

1. Open browser to `https://localhost:*/add-customer`
2. Verify page title shows "เพิ่มลูกค้า"
3. Fill in: เลขบัตรประจำตัว = `1234567890123`, ชื่อ = `ทดสอบ`, นามสกุล = `ลูกค้า`, วันเกิด, ที่อยู่, select จังหวัด/อำเภอ/ตำบล
4. Click บันทึก
5. Expected: success message "บันทึกข้อมูลลูกค้าเรียบร้อย"

- [ ] **Step 5: Smoke test — Search customer**

1. Navigate to `/search-customer`
2. Verify page title shows "ค้นหาลูกค้า"
3. Click ค้นหา without filters
4. Expected: table shows the record just created, column header "เลขบัตรประจำตัว"
5. Click ดูรายละเอียด on the row
6. Expected: drawer opens with title "รายละเอียดลูกค้า", shows all fields

- [ ] **Step 6: Final commit**

```bash
git add -A
git commit -m "chore: complete Citizen→Customer rename (DB + code + solution)"
```

---

## Out of Scope (not touched)

- `.devin/agents/*/AGENT.md` — agent descriptions referencing project name (optional cleanup)
- `docs/superpowers/plans/2026-07-05-citizen-management-ui.md` — historical plan doc, kept as record
- `data/` — mudblazor reference repo, unrelated
- `obj/`, `bin/`, `.vs/` — build artifacts, regenerated automatically
- Root folder `E:\WorkPlace\test\Citizen` — not renamed (per user decision)

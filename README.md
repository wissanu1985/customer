# Customer

ระบบจัดการข้อมูลลูกค้า (Customer Management) สำหรับเก็บและค้นหาข้อมูลประชาชนไทย
บน .NET 10 + Blazor Server + SQL Server โดยใช้สถาปัตยกรรม Clean Architecture และ CQRS

## สารบัญ

- [ภาพรวมระบบ](#ภาพรวมระบบ)
- [รูปแบบสถาปัตยกรรมของระบบ](#รูปแบบสถาปัตยกรรมของระบบ)
- [โครงสร้างโปรเจกต์](#โครงสร้างโปรเจกต์)
- [เทคโนโลยีที่ใช้](#เทคโนโลยีที่ใช้)
- [ข้อกำหนดเบื้องต้น](#ข้อกำหนดเบื้องต้น)
- [การติดตั้งและรัน](#การติดตั้งและรัน)
- [ฟีเจอร์หลัก](#ฟีเจอร์หลัก)
- [เอกสารเพิ่มเติม](#เอกสารเพิ่มเติม)

---

## ภาพรวมระบบ

ระบบบันทึกและค้นหาข้อมูลลูกค้า ประกอบด้วย:

- บันทึกข้อมูลลูกค้า (เลขบัตรประชาชน, ชื่อ-สกุล, วันเกิด, ที่อยู่ตามโครงสร้าง จังหวัด/อำเภอ/ตำบล/รหัสไปรษณีย์)
- แก้ไขข้อมูลลูกค้าที่มีอยู่
- ค้นหาลูกค้าตามเงื่อนไข
- อ้างอิงข้อมูลพื้นที่ (จังหวัด/อำเภอ/ตำบล) จากข้อมูลอ้างอิงของไทย
- บันทึกประวัติการเปลี่ยนแปลง (Audit) อัตโนมัติผ่าน EF Core interceptor

UI เป็น Blazor Server ใช้ Ant Design Blazor เป็น component library หลัก

---

## รูปแบบสถาปัตยกรรมของระบบ

ระบบนี้ใช้ **Clean Architecture** แบ่งเป็น **4 Layers** ทั้งหมดเขียนด้วย **C# / .NET 10**
โดย dependency ทิศทางเข้าหาศูนย์กลาง (Domain) เท่านั้น

```
                ┌────────────────────────────────────────────┐
                │              WebUi (Presentation)          │
                │   Blazor Server + Ant Design Blazor        │
                │   Razor components, Pages, DI wiring       │
                └──────────┬───────────────────┬─────────────┘
                           │                   │
                           ▼                   ▼
            ┌──────────────────────┐  ┌──────────────────────────┐
            │      Application     │  │      Infrastructure      │
            │  CQRS via Mediator   │  │  EF Core (SQL Server)    │
            │  FluentValidation    │◄─│  Repositories            │
            │  Pipeline behaviours │  │  Audit interceptor       │
            └──────────┬───────────┘  └──────────┬───────────────┘
                       │                         │
                       ▼                         │
                ┌──────────────────────────────────┐
                │              Domain              │
                │  Entities, Repository interfaces │
                │  AuditableEntity, IUnitOfWork    │
                │  (no external dependencies)      │
                └──────────────────────────────────┘
```

### สรุป Layer

| Layer | Project | ภาษา / เฟรมเวิร์ก | บทบาท |
|---|---|---|---|
| **Domain** | `src/Domain/Domain.csproj` | C# / .NET 10 + EF Core annotations | Entity, Repository interface, `AuditableEntity`, `IUnitOfWork` — ไม่มี dependency ภายนอก |
| **Application** | `src/Application/Application.csproj` | C# / .NET 10 + **Mediator** (Source-gen CQRS) + **FluentValidation** | Use case / Feature — Commands, Queries, Handlers, Validators, Pipeline Behaviours (Validation, Performance, UnhandledException) |
| **Infrastructure** | `src/Infrastructure/Infrastructure.csproj` | C# / .NET 10 + EF Core SqlServer | ติดต่อ DB, Repository implementation, `AuditSaveChangesInterceptor`, `CurrentUserProvider`, EF Configurations |
| **WebUi (Presentation)** | `src/WebUi/WebUi.csproj` | C# / **Blazor Server** (.NET 10) + **Ant Design Blazor** | UI — Razor components, Pages (`Home`, `AddCustomer`, `EditCustomer`, `SearchCustomer`), DI wiring ใน `Program.cs` |

### ทิศทาง Dependency

```
WebUi ──┬──► Application ──► Domain
        └──► Infrastructure ──► Application ──► Domain
```

- **Domain** ไม่พึ่งพอใคร (pure domain model)
- **Application** พึ่ง Domain เท่านั้น (ใช้ interface ของ Repository / UnitOfWork)
- **Infrastructure** พึ่ง Application (เพื่อให้ DI ลงทะเบียน implementation ได้) และ Domain
- **WebUi** พึ่ง Application + Infrastructure (เพื่อเรียก `AddApplicationServices()` / `AddInfrastructureServices()`)

### รูปแบบ CQRS

ใช้ **Mediator** (source-generated, zero-allocation) แทน MediatR (commercial)
แต่ละ feature อยู่ใน folder `Application/Features/<Feature>/{Commands,Queries}/<UseCase>/`
ประกอบด้วย 4 ไฟล์:

| ไฟล์ | หน้าที่ |
|---|---|
| `Request.cs` | Command/Query message (input) |
| `RequestHandler.cs` | Handler ที่ทำงานจริง คืน `Result<Response>` |
| `RequestValidator.cs` | FluentValidation rules |
| `Response.cs` | Output DTO |

Pipeline behaviours ที่ลงทะเบียนใน `Application/DependencyInjection.cs`:

1. `UnhandledExceptionBehaviour` — catch และ log exception
2. `ValidationBehaviour` — รัน validator ก่อน handler
3. `PerformanceBehaviour` — วัดเวลาที่ handler ใช้

---

## โครงสร้างโปรเจกต์

```
citizen/
├── Customer.slnx                 # Solution file (.slnx format)
├── README.md
├── .gitignore
├── docs/
│   ├── sql/                      # SQL migration scripts + README
│   │   ├── 01-create-schema.sql
│   │   ├── 02-seed-data.sql
│   │   ├── 03-health-check.sql
│   │   └── README.md
│   └── superpowers/plans/        # Planning documents
└── src/
    ├── Domain/
    │   ├── Common/               # AuditableEntity, IEntity, IUnitOfWork, IRepository
    │   ├── Entities/             # Customer, Province, District, SubDistrict, Audit
    │   └── Repositories/         # ICustomerRepository, IAuditRepository
    ├── Application/
    │   ├── Commons/              # Behaviours, Exceptions, Wrappers (Result, IPagedResult)
    │   ├── Features/
    │   │   ├── Customers/        # CreateCustomer, UpdateCustomer, GetCustomer, SearchCustomers
    │   │   └── Locations/        # GetAllProvinces, GetDistrictsByProvince, GetSubDistrictsByDistrict
    │   └── DependencyInjection.cs
    ├── Infrastructure/
    │   ├── Configurations/       # EF Core IEntityTypeConfiguration<T>
    │   ├── Interceptors/         # AuditSaveChangesInterceptor
    │   ├── Repositories/         # CustomerRepository, AuditRepository, BaseRepository, ReadOnly*
    │   ├── Services/             # CurrentUserProvider
    │   ├── contexts/             # CustomerContext (DbContext), UnitOfWork
    │   └── DependencyInjection.cs
    └── WebUi/
        ├── Components/
        │   ├── Pages/            # Home, AddCustomer, EditCustomer, SearchCustomer, Error, NotFound
        │   ├── Layout/           # MainLayout, NavMenu, ReconnectModal
        │   ├── App.razor
        │   └── _Imports.razor
        ├── Services/             # ErrorDialogService, ScopedMediator
        ├── wwwroot/              # Static assets (bootstrap, app.css, favicon)
        ├── Program.cs
        ├── appsettings.json
        └── appsettings.Development.json
```

---

## เทคโนโลยีที่ใช้

| กลุ่ม | เทคโนโลยี | เวอร์ชัน |
|---|---|---|
| Runtime | .NET | 10.0 |
| ภาษา | C# | (เวอร์ชันที่มากับ .NET 10) |
| UI | Blazor Server | .NET 10 |
| UI Components | Ant Design Blazor | 1.6.2 |
| ORM | EF Core + SqlServer | 10.0.6 |
| CQRS | Mediator (SourceGenerator) | 3.0.2 |
| Validation | FluentValidation | 12.1.1 |
| Database | Microsoft SQL Server | (ตั้งค่าใน `appsettings.json`) |
| Solution format | `.slnx` (XML-based) | — |

> **หมายเหตุ:** ใช้ `Mediator` (open-source, source-generated) แทน `MediatR` (commercial)
> เพื่อ zero-allocation และ build-time error checking

---

## ข้อกำหนดเบื้องต้น

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express หรือเวอร์ชันเต็ม) ที่เข้าถึงได้จากเครื่องที่รัน
- (optional) SQL Server Management Studio (SSMS) สำหรับรัน SQL scripts

---

## การติดตั้งและรัน

### 1. เตรียมฐานข้อมูล

รัน SQL scripts ตามลำดับใน `docs/sql/` (ดูรายละเอียดใน `docs/sql/README.md`):

```text
01-create-schema.sql  →  02-seed-data.sql  →  03-health-check.sql
```

หรือใช้ sqlcmd:

```powershell
$sqlcmd = "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE"
& $sqlcmd -S localhost -U sa -P "your_password" -C -d Customer -i "docs/sql/01-create-schema.sql"
& $sqlcmd -S localhost -U sa -P "your_password" -C -d Customer -i "docs/sql/02-seed-data.sql"
& $sqlcmd -S localhost -U sa -P "your_password" -C -d Customer -i "docs/sql/03-health-check.sql"
```

### 2. ตั้งค่า connection string

แก้ `src/WebUi/appsettings.json` หรือ `appsettings.Development.json`
ให้ตรงกับ SQL Server ของคุณ:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=localhost;Initial Catalog=Customer;User ID=sa;Password=your_password;Encrypt=False;Trust Server Certificate=True"
}
```

> **คำเตือน:** อย่า commit connection string จริงเข้า repository
> ใช้ `appsettings.Development.json` (ถูก ignore โดยปกติ) หรือ User Secrets

### 3. รันแอป

```powershell
dotnet restore Customer.slnx
dotnet build Customer.slnx
dotnet run --project src/WebUi/WebUi.csproj
```

เปิดเบราว์เซอร์ที่ URL ที่แสดงใน console (ปกติ `https://localhost:5xxx` หรือ `http://localhost:5xxx`)

---

## ฟีเจอร์หลัก

| หน้า | ไฟล์ | รายละเอียด |
|---|---|---|
| หน้าหลัก | `Components/Pages/Home.razor` | Landing page |
| เพิ่มลูกค้า | `Components/Pages/AddCustomer.razor` | Form บันทึกข้อมูลลูกค้าใหม่ |
| แก้ไขลูกค้า | `Components/Pages/EditCustomer.razor` | แก้ไขข้อมูลลูกค้าที่มีอยู่ |
| ค้นหาลูกค้า | `Components/Pages/SearchCustomer.razor` | ค้นหาตามเงื่อนไข |
| ข้อผิดพลาด | `Components/Pages/Error.razor` | Global error page |
| ไม่พบหน้า | `Components/Pages/NotFound.razor` | 404 page |

### Audit (บันทึกประวัติการเปลี่ยนแปลง)

`AuditSaveChangesInterceptor` ทำงานอัตโนมัติทุกครั้งที่มี `SaveChanges`
โดยบันทึก entity ที่เปลี่ยนแปลง (Add/Update/Delete) ลงในตาราง `dbo.Audit`
เป็น JSON ที่เก็บ Thai text ได้ถูกต้อง (Thai-safe JSON encoding)

---

## เอกสารเพิ่มเติม

- [Database migration scripts](docs/sql/README.md) — วิธีรัน SQL scripts แบบ step-by-step

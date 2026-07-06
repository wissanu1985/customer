/* ============================================================================
   01-create-schema.sql
   ----------------------------------------------------------------------------
   Creates the Customer database schema: 5 dbo tables + indexes + FKs.
   Idempotent — safe to re-run (drops existing objects first).

   Order:  CREATE DB  ->  Tables (parent before child)  ->  Indexes  ->  FKs
   Run on : target SQL Server (sqlcmd / SSMS)
   Source : reverse-engineered from existing Customer DB (EF Core Power Tools)
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ------------------------------------------------------------------ */
/* 0. Database (optional — uncomment if you need a fresh catalog)     */
/* ------------------------------------------------------------------ */
/*
IF DB_ID('Customer') IS NULL
    CREATE DATABASE Customer;
GO
USE Customer;
GO
*/

/* ------------------------------------------------------------------ */
/* 1. Drop existing objects (idempotent rerun)                        */
/*    Order: child first, then parent.                                */
/* ------------------------------------------------------------------ */
IF OBJECT_ID('dbo.SubDistricts', 'U') IS NOT NULL DROP TABLE dbo.SubDistricts;
IF OBJECT_ID('dbo.Districts',     'U') IS NOT NULL DROP TABLE dbo.Districts;
IF OBJECT_ID('dbo.Provinces',     'U') IS NOT NULL DROP TABLE dbo.Provinces;
IF OBJECT_ID('dbo.Customers',     'U') IS NOT NULL DROP TABLE dbo.Customers;
IF OBJECT_ID('dbo.Audit',         'U') IS NOT NULL DROP TABLE dbo.Audit;
GO

/* ------------------------------------------------------------------ */
/* 2. Tables — parent first, child later (FK order)                   */
/* ------------------------------------------------------------------ */

/* 2.1 dbo.Audit — audit log (no FKs, standalone)
   Schema v2: single [Values] column replaces OldValues/NewValues.
   See 04-audit-single-values-column.sql for migration from v1. */
CREATE TABLE dbo.Audit
(
    Id           UNIQUEIDENTIFIER NOT NULL,
    Seq          INT              NOT NULL CONSTRAINT DF_Audit_Seq           DEFAULT (0),
    EntityType   NVARCHAR(100)    NOT NULL,
    EntityId     NVARCHAR(50)     NOT NULL,
    Action       NVARCHAR(50)     NOT NULL,
    [Values]     NVARCHAR(4000)   NULL,
    ChangedBy    NVARCHAR(200)    NULL,
    TableName    NVARCHAR(100)    NULL,
    AuditType    NVARCHAR(50)     NOT NULL,
    Timestamp    DATETIME2        NOT NULL CONSTRAINT DF_Audit_Timestamp     DEFAULT (getutcdate()),
    CreatedDate  DATETIME2        NOT NULL CONSTRAINT DF_Audit_CreatedDate  DEFAULT (getutcdate()),
    CreatedBy    NVARCHAR(200)    NOT NULL CONSTRAINT DF_Audit_CreatedBy    DEFAULT (N'System'),
    UpdatedDate  DATETIME2        NULL,
    UpdatedBy    NVARCHAR(200)    NULL,
    DeletedDate  DATETIME2        NULL,
    DeletedBy    NVARCHAR(200)    NULL,
    IsDeleted    BIT              NOT NULL CONSTRAINT DF_Audit_IsDeleted     DEFAULT (0),
    RowVersion   ROWVERSION       NOT NULL,
    CONSTRAINT PK__Audit__3214EC070E721ED5 PRIMARY KEY CLUSTERED (Id)
);
GO

/* 2.2 dbo.Customers — main business table */
CREATE TABLE dbo.Customers
(
    Id            UNIQUEIDENTIFIER NOT NULL,
    Seq           INT              NOT NULL CONSTRAINT DF_Customers_Seq           DEFAULT (0),
    NationalId    NVARCHAR(13)     NOT NULL,
    FirstName     NVARCHAR(100)    NOT NULL CONSTRAINT DF_Customers_FirstName    DEFAULT (N''),
    LastName      NVARCHAR(100)    NOT NULL CONSTRAINT DF_Customers_LastName     DEFAULT (N''),
    BirthDate     DATE             NOT NULL,
    AddressLine1  NVARCHAR(300)    NOT NULL CONSTRAINT DF_Customers_AddressLine1 DEFAULT (N''),
    SubDistrict   NVARCHAR(100)    NOT NULL CONSTRAINT DF_Customers_SubDistrict  DEFAULT (N''),
    District      NVARCHAR(100)    NOT NULL CONSTRAINT DF_Customers_District     DEFAULT (N''),
    Province      NVARCHAR(100)    NOT NULL CONSTRAINT DF_Customers_Province     DEFAULT (N''),
    PostalCode    NVARCHAR(10)     NOT NULL CONSTRAINT DF_Customers_PostalCode   DEFAULT (N''),
    IdCardImage   NVARCHAR(MAX)    NULL,
    CreatedDate   DATETIME2        NOT NULL CONSTRAINT DF_Customers_CreatedDate  DEFAULT (getutcdate()),
    CreatedBy     NVARCHAR(100)    NOT NULL CONSTRAINT DF_Customers_CreatedBy    DEFAULT (N'System'),
    UpdatedDate   DATETIME2        NULL,
    UpdatedBy     NVARCHAR(100)    NULL,
    DeletedDate   DATETIME2        NULL,
    DeletedBy     NVARCHAR(100)    NULL,
    IsDeleted     BIT              NOT NULL CONSTRAINT DF_Customers_IsDeleted    DEFAULT (0),
    RowVersion    ROWVERSION       NOT NULL,
    CONSTRAINT PK__Customers__3214EC07FC6943F7 PRIMARY KEY CLUSTERED (Id)
);
GO

/* 2.3 dbo.Provinces — reference (parent of Districts) */
CREATE TABLE dbo.Provinces
(
    ProvinceID    INT              NOT NULL,
    ProvinceThai  NVARCHAR(100)    NOT NULL,
    ProvinceEng   NVARCHAR(100)    NOT NULL,
    CONSTRAINT PK__Province__FD0A6FA3434E1134 PRIMARY KEY CLUSTERED (ProvinceID)
);
GO

/* 2.4 dbo.Districts — reference (child of Provinces, parent of SubDistricts) */
CREATE TABLE dbo.Districts
(
    DistrictID        INT           NOT NULL,
    ProvinceID        INT           NOT NULL,
    DistrictThai      NVARCHAR(100) NOT NULL,
    DistrictEng       NVARCHAR(100) NOT NULL,
    DistrictThaiShort NVARCHAR(100) NOT NULL,
    DistrictEngShort  NVARCHAR(100) NOT NULL,
    CONSTRAINT PK__District__85FDA4A66A12FBD5 PRIMARY KEY CLUSTERED (DistrictID)
);
GO

/* 2.5 dbo.SubDistricts — reference (child of Districts) */
CREATE TABLE dbo.SubDistricts
(
    TambonID        INT           NOT NULL,
    DistrictID      INT           NOT NULL,
    TambonThai      NVARCHAR(200) NOT NULL,
    TambonEng       NVARCHAR(200) NOT NULL,
    TambonThaiShort NVARCHAR(100) NOT NULL,
    TambonEngShort  NVARCHAR(100) NOT NULL,
    PostalCode      NVARCHAR(10)  NOT NULL,
    CONSTRAINT PK__SubDistr__ADAF7E17809473E3 PRIMARY KEY CLUSTERED (TambonID)
);
GO

/* ------------------------------------------------------------------ */
/* 3. Foreign Keys                                                    */
/* ------------------------------------------------------------------ */
ALTER TABLE dbo.Districts
    ADD CONSTRAINT FK_Districts_Provinces
    FOREIGN KEY (ProvinceID) REFERENCES dbo.Provinces (ProvinceID);
GO

ALTER TABLE dbo.SubDistricts
    ADD CONSTRAINT FK_SubDistricts_Districts
    FOREIGN KEY (DistrictID) REFERENCES dbo.Districts (DistrictID);
GO

/* ------------------------------------------------------------------ */
/* 4. Nonclustered Indexes                                            */
/* ------------------------------------------------------------------ */

/* 4.1 Audit — single-column seek helpers */
CREATE NONCLUSTERED INDEX IX_Audit_Action     ON dbo.Audit (Action)     ON [PRIMARY];
CREATE NONCLUSTERED INDEX IX_Audit_ChangedBy  ON dbo.Audit (ChangedBy)  ON [PRIMARY];
CREATE NONCLUSTERED INDEX IX_Audit_EntityId   ON dbo.Audit (EntityId)   ON [PRIMARY];
CREATE NONCLUSTERED INDEX IX_Audit_EntityType ON dbo.Audit (EntityType) ON [PRIMARY];
CREATE NONCLUSTERED INDEX IX_Audit_Timestamp  ON dbo.Audit (Timestamp)  ON [PRIMARY];
GO

/* 4.2 Customers — unique business key */
CREATE UNIQUE NONCLUSTERED INDEX UX_Customers_NationalId
    ON dbo.Customers (NationalId) ON [PRIMARY];
GO

/* 4.3 Customers — covering indexes for SearchCustomers (see optimize-customer-indexes.sql) */
CREATE NONCLUSTERED INDEX IX_Customers_CreatedDate_DESC
    ON dbo.Customers (CreatedDate DESC)
    INCLUDE (NationalId, FirstName, LastName, BirthDate,
             AddressLine1, SubDistrict, District, Province, PostalCode, IsDeleted)
    ON [PRIMARY];
GO

CREATE NONCLUSTERED INDEX IX_Customers_Province
    ON dbo.Customers (Province, CreatedDate DESC)
    INCLUDE (NationalId, FirstName, LastName, BirthDate,
             AddressLine1, SubDistrict, District, PostalCode, IsDeleted)
    ON [PRIMARY];
GO

CREATE NONCLUSTERED INDEX IX_Customers_PostalCode
    ON dbo.Customers (PostalCode, CreatedDate DESC)
    INCLUDE (NationalId, FirstName, LastName, BirthDate,
             AddressLine1, SubDistrict, District, Province, IsDeleted)
    ON [PRIMARY];
GO

CREATE NONCLUSTERED INDEX IX_Customers_NationalId
    ON dbo.Customers (NationalId)
    INCLUDE (FirstName, LastName, BirthDate, AddressLine1,
             SubDistrict, District, Province, PostalCode, CreatedDate, IsDeleted)
    ON [PRIMARY];
GO

/* ------------------------------------------------------------------ */
/* 5. Done                                                             */
/* ------------------------------------------------------------------ */
PRINT 'Schema created: dbo.Audit, dbo.Customers, dbo.Provinces, dbo.Districts, dbo.SubDistricts';
GO

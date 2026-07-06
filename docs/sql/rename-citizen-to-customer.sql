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

/* ============================================================================
   03-health-check.sql
   ----------------------------------------------------------------------------
   Verifies the Customer database is ready for the app to start.
   Run AFTER 01-create-schema.sql + 02-seed-data.sql.

   Checks:
     1. All 5 expected tables exist
     2. Row counts match expected seed values
     3. All required indexes present
     4. Foreign keys intact
     5. Unique constraint on Customers.NationalId works
     6. Default constraints work (CreatedDate, IsDeleted, etc.)
     7. Soft-delete flag sanity (no row has IsDeleted=1 without DeletedDate)
     8-9. Reference data integrity (Districts→Provinces, SubDistricts→Districts)
     10. Audit schema v2 ([Values] present, OldValues/NewValues removed)
   Exit code 0 = healthy, non-zero = problems (via RAISERROR at the end).
   ============================================================================ */

SET NOCOUNT ON;
GO

/* Shared error counter — temp table survives GO within the same session. */
CREATE TABLE #Errors (Id INT IDENTITY(1,1), Message NVARCHAR(400));
GO

/* ------------------------------------------------------------------ */
/* 1. Tables exist                                                    */
/* ------------------------------------------------------------------ */
IF OBJECT_ID('dbo.Audit',        'U') IS NULL INSERT INTO #Errors VALUES (N'1. Missing table: dbo.Audit');
IF OBJECT_ID('dbo.Customers',    'U') IS NULL INSERT INTO #Errors VALUES (N'1. Missing table: dbo.Customers');
IF OBJECT_ID('dbo.Provinces',    'U') IS NULL INSERT INTO #Errors VALUES (N'1. Missing table: dbo.Provinces');
IF OBJECT_ID('dbo.Districts',    'U') IS NULL INSERT INTO #Errors VALUES (N'1. Missing table: dbo.Districts');
IF OBJECT_ID('dbo.SubDistricts', 'U') IS NULL INSERT INTO #Errors VALUES (N'1. Missing table: dbo.SubDistricts');

IF NOT EXISTS (SELECT 1 FROM #Errors WHERE Message LIKE N'1.%')
    PRINT '[OK] 1. All 5 expected tables present.';
GO

/* ------------------------------------------------------------------ */
/* 2. Row counts                                                      */
/* ------------------------------------------------------------------ */
DECLARE @Expected TABLE (TableName SYSNAME, ExpectedRows INT);
INSERT INTO @Expected VALUES
    (N'Provinces',     77),
    (N'Districts',    928),
    (N'SubDistricts',7436),
    (N'Customers',      3);  -- Audit excluded: log table grows on its own

DECLARE @Actual INT, @Exp INT, @Tbl SYSNAME;

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT TableName, ExpectedRows FROM @Expected;

OPEN cur;
FETCH NEXT FROM cur INTO @Tbl, @Exp;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @Actual = SUM(p.rows)
    FROM sys.partitions p
    WHERE p.object_id = OBJECT_ID('dbo.' + @Tbl)
      AND p.index_id IN (0, 1);

    IF @Actual IS NULL SET @Actual = -1;

    IF @Actual = @Exp
        PRINT '[OK] 2. ' + @Tbl + ': ' + CAST(@Actual AS VARCHAR(10)) + ' rows (expected ' + CAST(@Exp AS VARCHAR(10)) + ').';
    ELSE
        INSERT INTO #Errors VALUES (N'2. ' + @Tbl + N': ' + CAST(@Actual AS NVARCHAR(10)) + N' rows (expected ' + CAST(@Exp AS NVARCHAR(10)) + N').');

    FETCH NEXT FROM cur INTO @Tbl, @Exp;
END
CLOSE cur;
DEALLOCATE cur;
GO

/* ------------------------------------------------------------------ */
/* 3. Required indexes present                                        */
/* ------------------------------------------------------------------ */
DECLARE @ExpectedIndexes TABLE (TableName SYSNAME, IndexName SYSNAME);
INSERT INTO @ExpectedIndexes VALUES
    (N'Audit',        N'PK__Audit__3214EC070E721ED5'),
    (N'Audit',        N'IX_Audit_Action'),
    (N'Audit',        N'IX_Audit_ChangedBy'),
    (N'Audit',        N'IX_Audit_EntityId'),
    (N'Audit',        N'IX_Audit_EntityType'),
    (N'Audit',        N'IX_Audit_Timestamp'),
    (N'Customers',    N'PK__Customers__3214EC07FC6943F7'),
    (N'Customers',    N'UX_Customers_NationalId'),
    (N'Customers',    N'IX_Customers_CreatedDate_DESC'),
    (N'Customers',    N'IX_Customers_Province'),
    (N'Customers',    N'IX_Customers_PostalCode'),
    (N'Customers',    N'IX_Customers_NationalId'),
    (N'Districts',    N'PK__District__85FDA4A66A12FBD5'),
    (N'Provinces',    N'PK__Province__FD0A6FA3434E1134'),
    (N'SubDistricts', N'PK__SubDistr__ADAF7E17809473E3');

INSERT INTO #Errors
SELECT N'3. Missing index: ' + e.TableName + N'.' + e.IndexName
FROM @ExpectedIndexes e
WHERE NOT EXISTS (
    SELECT 1 FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID('dbo.' + e.TableName)
      AND i.name = e.IndexName
);

DECLARE @IdxCount INT = (SELECT COUNT(*) FROM @ExpectedIndexes);
IF NOT EXISTS (SELECT 1 FROM #Errors WHERE Message LIKE N'3.%')
    PRINT '[OK] 3. All ' + CAST(@IdxCount AS VARCHAR(10)) + ' required indexes present.';
GO

/* ------------------------------------------------------------------ */
/* 4. Foreign keys intact                                             */
/* ------------------------------------------------------------------ */
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_Districts_Provinces'
      AND parent_object_id = OBJECT_ID('dbo.Districts')
      AND referenced_object_id = OBJECT_ID('dbo.Provinces')
)
    INSERT INTO #Errors VALUES (N'4. Missing FK: FK_Districts_Provinces');

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_SubDistricts_Districts'
      AND parent_object_id = OBJECT_ID('dbo.SubDistricts')
      AND referenced_object_id = OBJECT_ID('dbo.Districts')
)
    INSERT INTO #Errors VALUES (N'4. Missing FK: FK_SubDistricts_Districts');

IF NOT EXISTS (SELECT 1 FROM #Errors WHERE Message LIKE N'4.%')
    PRINT '[OK] 4. Both foreign keys present (FK_Districts_Provinces, FK_SubDistricts_Districts).';
GO

/* ------------------------------------------------------------------ */
/* 5. Unique constraint on Customers.NationalId                       */
/* ------------------------------------------------------------------ */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.Customers')
      AND name = 'UX_Customers_NationalId'
      AND is_unique = 1
)
    INSERT INTO #Errors VALUES (N'5. UX_Customers_NationalId missing or not unique.');

IF NOT EXISTS (SELECT 1 FROM #Errors WHERE Message LIKE N'5.%')
    PRINT '[OK] 5. UX_Customers_NationalId is unique.';
GO

/* ------------------------------------------------------------------ */
/* 6. Default constraints work (smoke test, rolled back)              */
/* ------------------------------------------------------------------ */
BEGIN TRANSACTION;
    DECLARE @TestId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.Customers
        (Id, NationalId, FirstName, LastName, BirthDate, AddressLine1)
    VALUES
        (@TestId, N'HC0000000000', N'__test__', N'__test__', '2000-01-01', N'__test__');

    DECLARE @GotCreatedBy NVARCHAR(100), @GotIsDeleted BIT;
    SELECT @GotCreatedBy = CreatedBy, @GotIsDeleted = IsDeleted
    FROM dbo.Customers WHERE Id = @TestId;

    IF @GotCreatedBy = N'System' AND @GotIsDeleted = 0
        PRINT '[OK] 6. Default constraints fire (CreatedBy=''System'', IsDeleted=0).';
    ELSE
        INSERT INTO #Errors VALUES (N'6. Default constraints not firing. CreatedBy=' + ISNULL(@GotCreatedBy, N'NULL') + N', IsDeleted=' + CAST(ISNULL(@GotIsDeleted, -1) AS NVARCHAR(10)) + N'.');

    -- cleanup (rollback will undo the insert anyway)
ROLLBACK TRANSACTION;
GO

/* ------------------------------------------------------------------ */
/* 7. Soft-delete sanity (no IsDeleted=1 without DeletedDate)         */
/* ------------------------------------------------------------------ */
IF EXISTS (SELECT 1 FROM dbo.Customers WHERE IsDeleted = 1 AND DeletedDate IS NULL)
    INSERT INTO #Errors
    SELECT N'7. ' + CAST(COUNT(*) AS NVARCHAR(10)) + N' soft-deleted Customer rows missing DeletedDate.'
    FROM dbo.Customers WHERE IsDeleted = 1 AND DeletedDate IS NULL;

IF NOT EXISTS (SELECT 1 FROM #Errors WHERE Message LIKE N'7.%')
    PRINT '[OK] 7. No soft-delete orphans (IsDeleted=1 without DeletedDate).';
GO

/* ------------------------------------------------------------------ */
/* 8. Reference data integrity — Districts → Provinces                */
/* ------------------------------------------------------------------ */
IF EXISTS (
    SELECT 1
    FROM dbo.Districts d
    LEFT JOIN dbo.Provinces p ON d.ProvinceID = p.ProvinceID
    WHERE p.ProvinceID IS NULL
)
    INSERT INTO #Errors
    SELECT N'8. ' + CAST(COUNT(*) AS NVARCHAR(10)) + N' District rows reference missing Provinces.'
    FROM dbo.Districts d
    LEFT JOIN dbo.Provinces p ON d.ProvinceID = p.ProvinceID
    WHERE p.ProvinceID IS NULL;

IF NOT EXISTS (SELECT 1 FROM #Errors WHERE Message LIKE N'8.%')
    PRINT '[OK] 8. All Districts point to existing Provinces.';
GO

/* ------------------------------------------------------------------ */
/* 9. Reference data integrity — SubDistricts → Districts             */
/* ------------------------------------------------------------------ */
IF EXISTS (
    SELECT 1
    FROM dbo.SubDistricts s
    LEFT JOIN dbo.Districts d ON s.DistrictID = d.DistrictID
    WHERE d.DistrictID IS NULL
)
    INSERT INTO #Errors
    SELECT N'9. ' + CAST(COUNT(*) AS NVARCHAR(10)) + N' SubDistrict rows reference missing Districts.'
    FROM dbo.SubDistricts s
    LEFT JOIN dbo.Districts d ON s.DistrictID = d.DistrictID
    WHERE d.DistrictID IS NULL;

IF NOT EXISTS (SELECT 1 FROM #Errors WHERE Message LIKE N'9.%')
    PRINT '[OK] 9. All SubDistricts point to existing Districts.';
GO

/* ------------------------------------------------------------------ */
/* 10. Audit schema v2 — [Values] present, OldValues/NewValues gone  */
/* ------------------------------------------------------------------ */
IF COL_LENGTH('dbo.Audit', 'Values') IS NULL
    INSERT INTO #Errors VALUES (N'10. dbo.Audit missing [Values] column (schema v2 required).');

IF COL_LENGTH('dbo.Audit', 'OldValues') IS NOT NULL
    INSERT INTO #Errors VALUES (N'10. dbo.Audit still has OldValues column (run 04-audit-single-values-column.sql).');

IF COL_LENGTH('dbo.Audit', 'NewValues') IS NOT NULL
    INSERT INTO #Errors VALUES (N'10. dbo.Audit still has NewValues column (run 04-audit-single-values-column.sql).');

IF NOT EXISTS (SELECT 1 FROM #Errors WHERE Message LIKE N'10.%')
    PRINT '[OK] 10. Audit schema v2: [Values] present, OldValues/NewValues removed.';
GO

/* ------------------------------------------------------------------ */
/* Summary                                                            */
/* ------------------------------------------------------------------ */
DECLARE @ErrCount INT = (SELECT COUNT(*) FROM #Errors);

PRINT '';
PRINT '========================================';
PRINT '  Health check complete.';
PRINT '  Errors: ' + CAST(@ErrCount AS VARCHAR(10));
PRINT '========================================';

IF @ErrCount > 0
BEGIN
    PRINT '';
    PRINT 'Failed checks:';
    SELECT '  - ' + Message FROM #Errors ORDER BY Id;

    RAISERROR('Health check FAILED — fix the issues above before starting the app.', 16, 1);
END
ELSE
    PRINT 'Database is ready. App can start.';

DROP TABLE #Errors;
GO

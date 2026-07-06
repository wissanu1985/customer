# Customer Database — Migration Scripts

Scripts to bootstrap the `Customer` database on a fresh SQL Server instance.
Run in order. Each script is idempotent (safe to re-run) except `02-seed-data.sql`,
which expects empty tables (re-run `01` first to reset).

## Files

| # | File | Purpose |
|---|------|---------|
| 1 | `01-create-schema.sql` | Creates 5 dbo tables + indexes + FKs. Drops existing objects first. |
| 2 | `02-seed-data.sql`     | Inserts reference data (Provinces, Districts, SubDistricts) + Customers. |
| 3 | `03-health-check.sql`  | Verifies schema, row counts, indexes, FKs, defaults, integrity. |

## Run order

```text
Fresh install:
  01-create-schema.sql  →  02-seed-data.sql  →  03-health-check.sql

```

## Run in SSMS (SQL Server Management Studio)

### ก่อนเริ่ม

1. เปิด **SSMS** → เชื่อมต่อ SQL Server (`localhost` หรือชื่อ server เป้าหมาย)
   - Authentication: **SQL Server Authentication**
   - Login: `sa` | Password: รหัสของคุณ
2. ถ้ายังไม่มี database `Customer`:
   - คลิกขวา **Databases** → **New Database...**
   - ชื่อ: `Customer` → กด **OK**
   - หรือเปิด `01-create-schema.sql` → uncomment บล็อก `CREATE DATABASE` บนสุด → รันบล็อกนั้นก่อน

### ขั้นตอนการรัน (Fresh install)

> **สำคัญ**: รันทีละไฟล์ตามลำดับ อย่าข้ามขั้น

#### Step 1 — สร้าง schema

1. เปิดไฟล์ `01-create-schema.sql` (File → Open → File... → เลือกไฟล์)
2. ดูแถบ toolbar ด้านบน → เลือก database เป็น **`Customer`** ใน dropdown
   (ถ้าเลือกผิดเป็น `master` จะสร้างตารางผิดที่)
3. กด **F5** หรือปุ่ม **Execute** (!) บน toolbar
4. ผลที่คาดหวานใน **Messages** tab:
   ```
   Schema created: dbo.Audit, dbo.Customers, dbo.Provinces, dbo.Districts, dbo.SubDistricts
   ```

#### Step 2 — ใส่ข้อมูล seed

1. เปิดไฟล์ `02-seed-data.sql`
2. ตรวจ database ใน toolbar ว่าเป็น **`Customer`**
3. กด **F5**
   - ไฟล์ใหญ่ ~2 MB (8,400+ INSERT statements) — รอ 30–60 วินาที
   - ถ้า SSMS ถาม "File too large to open" → ใช้ sqlcmd แทน (ดู section ด้านล่าง)
4. ผลที่คาดหวาน:
   ```
   Seed data inserted.
   ```

#### Step 3 — ตรวจสอบ

1. เปิดไฟล์ `03-health-check.sql`
2. ตรวจ database ว่าเป็น **`Customer`**
3. กด **F5**
4. ดูผลใน **Messages** tab — ต้องเห็นครบ 10 บรรทัด `[OK]` และบรรทัดสุดท้าย:
   ```
   ========================================
     Health check complete.
     Errors: 0
   ========================================
   Database is ready. App can start.
   ```
   - ถ้ามี `[FAIL]` → อ่านข้อความแล้วแก้ก่อนเริ่ม app

5. รัน `03-health-check.sql` อีกครั้ง → check 10 ต้องขึ้น `[OK]`

### รันซ้ำ (re-run)

| Script | รันซ้ำได้? | ผลกระทบ |
|--------|-----------|---------|
| `01-create-schema.sql` | ได้ | **DROP + CREATE** ล้างข้อมูลทั้งหมด ระวัง! |
| `02-seed-data.sql`     | ไม่ได้ (ถ้ามีข้อมูลแล้ว) | จะ PK/FK ซ้ำ → error ต้องรัน 01 ก่อน |
| `03-health-check.sql`  | ได้ | อ่านอย่างเดียว ปลอดภัย |

### เคล็ดลับ SSMS

- **เลือก database ผิดทุกครั้ง?** → เปิดไฟล์ → กด `Ctrl+M` ไม่ช่วย แต่ใส่ `USE Customer;` บนสุดของ query tab ก่อนกด F5 ได้
- **ไฟล์ 02 ใหญ่เปิดไม่ได้?** → ใช้ sqlcmd (section ด้านล่าง) หรือแบ่งรันทีละส่วน
- **ดูผลเป็นตาราง** → Results tab แสดง output ของ SELECT; Messages tab แสดง PRINT/RAISERROR
- **ยกเลิกการรัน** → กด **Alt+Break** หรือปุ่มสี่เหลี่ยมแดงบน toolbar

## Quick start (sqlcmd)

```powershell
$server   = "localhost"
$user     = "sa"
$password = "your_password"   # do not commit real passwords
$db       = "Customer"
$sqlcmd   = "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE"

# 0. (optional) create the database
& $sqlcmd -S $server -U $user -P $password -C -Q "CREATE DATABASE $db;"

# 1. schema
& $sqlcmd -S $server -U $user -P $password -C -d $db -i "01-create-schema.sql"

# 2. seed data
& $sqlcmd -S $server -U $user -P $password -C -d $db -i "02-seed-data.sql"

# 3. health check (exit code 0 = healthy)
& $sqlcmd -S $server -U $user -P $password -C -d $db -i "03-health-check.sql"

```

## What gets migrated

| Table | Rows | Notes |
|-------|------|-------|
| `dbo.Provinces`     |   77 | Thai provinces (reference) |
| `dbo.Districts`     |  928 | Districts (reference, FK → Provinces) |
| `dbo.SubDistricts`  | 7436 | Sub-districts (reference, FK → Districts) |
| `dbo.Customers`     |    3 | Customer rows (full data) |
| `dbo.Audit`         |    0 | Schema only — audit history not migrated |
| `HangFire.*`        |   —  | Not migrated — recreated automatically when the app starts |

## Regenerating `02-seed-data.sql`

The seed file is auto-generated from the source DB via `_generate-inserts.sql`
(kept for reproducibility — do not run it on the target DB).

```powershell
& $sqlcmd -S $server -U $user -P $password -C -d $db `
    -i "_generate-inserts.sql" -o "02-seed-data.sql" -y 0 -Y 0 -f 65001
```

`-y 0 -Y 0` = unlimited column width (otherwise INSERT lines truncate at 80 chars).
`-f 65001`  = UTF-8 output (preserves Thai text).

## Notes

- `01-create-schema.sql` includes a commented-out `CREATE DATABASE Customer;` block
  at the top — uncomment if the catalog does not exist yet.
- All `NVARCHAR` columns use `N'...'` literals in INSERTs to preserve Thai text.
- `RowVersion` columns are `ROWVERSION` (auto-managed by SQL Server) — never inserted.
- The clustered PK on `dbo.Customers` is `Guid Id` (random) — expect fragmentation
  on heavy inserts. See `optimize-customer-indexes.sql` for advanced tuning.

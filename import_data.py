import openpyxl
import pyodbc
from collections import OrderedDict

EXCEL_PATH = r"c:\Users\Moo\Downloads\ThepExcel-Thailand-Tambon.xlsx"
CONN_STR = "DRIVER={ODBC Driver 17 for SQL Server};SERVER=localhost;DATABASE=Citizen;UID=sa;PWD=Permsri@1;TrustServerCertificate=yes"

wb = openpyxl.load_workbook(EXCEL_PATH, read_only=True, data_only=True)
ws = wb["TambonDatabase"]

provinces = OrderedDict()
districts = OrderedDict()
subdistricts = []

for row in ws.iter_rows(min_row=2, values_only=True):
    tambon_id = row[0]
    if tambon_id is None:
        continue
    tambon_thai = row[1] or ""
    tambon_eng = row[2] or ""
    tambon_thai_short = row[3] or ""
    tambon_eng_short = row[4] or ""
    district_id_raw = row[5]
    district_thai = row[6] or ""
    district_eng = row[7] or ""
    district_thai_short = row[8] or ""
    district_eng_short = row[9] or ""
    province_id = row[10]
    province_thai = row[11] or ""
    province_eng = row[12] or ""
    postal_code = str(row[18] or "")

    if province_id is None:
        continue
    province_id = int(province_id)
    district_id = int(district_id_raw) if district_id_raw else 0
    tambon_id = int(tambon_id)

    if province_id not in provinces:
        provinces[province_id] = (province_thai, province_eng)

    if district_id not in districts:
        districts[district_id] = (province_id, district_thai, district_eng, district_thai_short, district_eng_short)

    subdistricts.append((tambon_id, district_id, tambon_thai, tambon_eng, tambon_thai_short, tambon_eng_short, postal_code))

wb.close()

print(f"Provinces: {len(provinces)}, Districts: {len(districts)}, SubDistricts: {len(subdistricts)}")

conn = pyodbc.connect(CONN_STR, autocommit=False)
cursor = conn.cursor()

try:
    cursor.execute("DELETE FROM dbo.SubDistricts")
    cursor.execute("DELETE FROM dbo.Districts")
    cursor.execute("DELETE FROM dbo.Provinces")

    for pid, (thai, eng) in provinces.items():
        cursor.execute("INSERT INTO dbo.Provinces (ProvinceID, ProvinceThai, ProvinceEng) VALUES (?, ?, ?)", pid, thai, eng)

    for did, (pid, thai, eng, thai_short, eng_short) in districts.items():
        cursor.execute("INSERT INTO dbo.Districts (DistrictID, ProvinceID, DistrictThai, DistrictEng, DistrictThaiShort, DistrictEngShort) VALUES (?, ?, ?, ?, ?, ?)", did, pid, thai, eng, thai_short, eng_short)

    batch = []
    for sd in subdistricts:
        batch.append(sd)
        if len(batch) >= 500:
            cursor.fast_executemany = True
            cursor.executemany("INSERT INTO dbo.SubDistricts (TambonID, DistrictID, TambonThai, TambonEng, TambonThaiShort, TambonEngShort, PostalCode) VALUES (?, ?, ?, ?, ?, ?, ?)", batch)
            batch = []
    if batch:
        cursor.fast_executemany = True
        cursor.executemany("INSERT INTO dbo.SubDistricts (TambonID, DistrictID, TambonThai, TambonEng, TambonThaiShort, TambonEngShort, PostalCode) VALUES (?, ?, ?, ?, ?, ?, ?)", batch)

    conn.commit()
    print("Import completed successfully!")
except Exception as e:
    conn.rollback()
    print(f"Error: {e}")
    raise
finally:
    cursor.close()
    conn.close()

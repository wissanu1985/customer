namespace Domain.Entities;

public class SubDistrict
{
    public int TambonID { get; set; }
    public int DistrictID { get; set; }
    public string TambonThai { get; set; } = null!;
    public string TambonEng { get; set; } = null!;
    public string TambonThaiShort { get; set; } = null!;
    public string TambonEngShort { get; set; } = null!;
    public string PostalCode { get; set; } = null!;

    public District District { get; set; } = null!;
}

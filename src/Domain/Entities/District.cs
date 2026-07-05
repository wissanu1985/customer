namespace Domain.Entities;

public class District
{
    public int DistrictID { get; set; }
    public int ProvinceID { get; set; }
    public string DistrictThai { get; set; } = null!;
    public string DistrictEng { get; set; } = null!;
    public string DistrictThaiShort { get; set; } = null!;
    public string DistrictEngShort { get; set; } = null!;

    public Province Province { get; set; } = null!;
    public ICollection<SubDistrict> SubDistricts { get; set; } = new List<SubDistrict>();
}

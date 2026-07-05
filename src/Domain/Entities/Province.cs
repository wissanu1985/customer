namespace Domain.Entities;

public class Province
{
    public int ProvinceID { get; set; }
    public string ProvinceThai { get; set; } = null!;
    public string ProvinceEng { get; set; } = null!;

    public ICollection<District> Districts { get; set; } = new List<District>();
}

namespace Application.Features.IdCardExtractions.Queries.ExtractIdCard;

public sealed class IdCardData
{
    public string? NationalId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? AddressLine1 { get; set; }
    public string? ProvinceName { get; set; }
    public string? DistrictName { get; set; }
    public string? SubDistrictName { get; set; }
    public string? PostalCode { get; set; }
}

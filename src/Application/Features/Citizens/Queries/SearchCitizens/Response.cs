namespace Application.Features.Citizens.Queries.SearchCitizens;

public sealed class CitizenSearchItem
{
    public Guid Id { get; set; }
    public string IdCardNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string SubDistrict { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

namespace Domain.Common;

public interface IAuditableEntity : IEntity
{
    string CreatedBy { get; set; }
    DateTime? UpdatedDate { get; set; }
    string? UpdatedBy { get; set; }
    DateTime? DeletedDate { get; set; }
    string? DeletedBy { get; set; }
    bool IsDeleted { get; set; }
}
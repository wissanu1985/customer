namespace Domain.Common;

public interface IEntity<TId>
{
    TId Id { get; }
}

public interface IEntity : IEntity<Guid>
{
    int Seq { get; set; }
    DateTime CreatedDate { get; set; }
}
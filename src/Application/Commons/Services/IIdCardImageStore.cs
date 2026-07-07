namespace Application.Commons.Services;

public interface IIdCardImageStore
{
    Task<string> SaveAsync(byte[] imageBytes, Guid customerId, CancellationToken cancellationToken = default);
    Task<byte[]> ReadAsync(string filePath, CancellationToken cancellationToken = default);
}

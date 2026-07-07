namespace Application.Commons.Services;

public interface IIdCardImageStore
{
    Task<string> SaveAsync(byte[] imageBytes, int customerId, CancellationToken cancellationToken = default);
    Task<byte[]> ReadAsync(string filePath, CancellationToken cancellationToken = default);
}

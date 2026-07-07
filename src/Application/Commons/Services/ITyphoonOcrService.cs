namespace Application.Commons.Services;

public interface ITyphoonOcrService
{
    Task<string> OcrAsync(byte[] imageBytes, string fileName, CancellationToken cancellationToken = default);
}

using Application.Commons.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Services.IdCardImageStore;

public sealed class IdCardImageStore : IIdCardImageStore
{
    private readonly string _storageRoot;
    private readonly AesImageEncryptor _encryptor;

    public IdCardImageStore(
        IConfiguration configuration,
        RecyclableMemoryStreamManager streamManager,
        IWebHostEnvironment env)
    {
        var relativePath = configuration["IdCardImageStore:StorageRelativePath"] ?? "files/idcards";
        _storageRoot = Path.Combine(env.ContentRootPath, relativePath);
        Directory.CreateDirectory(_storageRoot);

        var keyBase64 = configuration["IdCardImageStore:EncryptionKey"]
            ?? throw new InvalidOperationException("IdCardImageStore:EncryptionKey is not configured.");
        var key = Convert.FromBase64String(keyBase64);
        if (key.Length != 32)
            throw new InvalidOperationException("IdCardImageStore:EncryptionKey must decode to 32 bytes (AES-256).");

        _encryptor = new AesImageEncryptor(key, streamManager);
    }

    public async Task<string> SaveAsync(byte[] imageBytes, int customerId, CancellationToken cancellationToken = default)
    {
        var encrypted = _encryptor.Encrypt(imageBytes);
        var filePath = Path.Combine(_storageRoot, $"{customerId}.bin");
        await File.WriteAllBytesAsync(filePath, encrypted, cancellationToken);
        return filePath;
    }

    public async Task<byte[]> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var encrypted = await File.ReadAllBytesAsync(filePath, cancellationToken);
        return _encryptor.Decrypt(encrypted);
    }
}

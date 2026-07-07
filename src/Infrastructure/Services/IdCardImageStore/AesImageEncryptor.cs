using Microsoft.IO;
using System.Security.Cryptography;

namespace Infrastructure.Services.IdCardImageStore;

/// <summary>
/// AES-256-GCM encrypt/decrypt helper. Each call generates a random 12-byte IV
/// which is prepended to the ciphertext output.
/// </summary>
public sealed class AesImageEncryptor
{
    private readonly byte[] _key; // 32 bytes for AES-256
    private readonly RecyclableMemoryStreamManager _streamManager;

    public AesImageEncryptor(byte[] key, RecyclableMemoryStreamManager streamManager)
    {
        if (key.Length != 32) throw new ArgumentException("Key must be 32 bytes (AES-256).", nameof(key));
        _key = key;
        _streamManager = streamManager;
    }

    public byte[] Encrypt(byte[] plaintext)
    {
        var iv = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var ciphertext = new byte[plaintext.Length];

        using var aes = new AesGcm(_key, tagSizeInBytes: 16);
        aes.Encrypt(iv, plaintext, ciphertext, tag);

        // Output layout: [12-byte IV][16-byte tag][ciphertext]
        var output = new byte[12 + 16 + plaintext.Length];
        Buffer.BlockCopy(iv, 0, output, 0, 12);
        Buffer.BlockCopy(tag, 0, output, 12, 16);
        Buffer.BlockCopy(ciphertext, 0, output, 28, plaintext.Length);
        return output;
    }

    public byte[] Decrypt(byte[] encrypted)
    {
        if (encrypted.Length < 28) throw new CryptographicException("Encrypted payload too short.");

        var iv = new byte[12];
        var tag = new byte[16];
        var ciphertext = new byte[encrypted.Length - 28];

        Buffer.BlockCopy(encrypted, 0, iv, 0, 12);
        Buffer.BlockCopy(encrypted, 12, tag, 0, 16);
        Buffer.BlockCopy(encrypted, 28, ciphertext, 0, ciphertext.Length);

        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(_key, tagSizeInBytes: 16);
        aes.Decrypt(iv, ciphertext, tag, plaintext);
        return plaintext;
    }
}

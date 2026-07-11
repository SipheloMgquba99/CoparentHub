using CoparentHub.Domain.Common;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace CoparentHub.Infrastructure.Security
{
    public sealed class AesGcmFieldEncryptor : IFieldEncryptor
    {
        private const int NonceSizeBytes = 12;
        private const int TagSizeBytes = 16;

        private readonly byte[] _key;

        public AesGcmFieldEncryptor(IConfiguration config)
        {
            var keyBase64 = config["Encryption:Key"];

            if (string.IsNullOrWhiteSpace(keyBase64))
            {
                throw new InvalidOperationException(
                    "Configuration error: Encryption:Key is not set. This key encrypts " +
                    "children's names/dates of birth and event details at rest. Generate one " +
                    "with `openssl rand -base64 32` and set it via the Encryption__Key " +
                    "environment variable or `dotnet user-secrets` in production — never " +
                    "commit a production key to source control. See SECURITY.md.");
            }

            try
            {
                _key = Convert.FromBase64String(keyBase64);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(
                    "Configuration error: Encryption:Key must be a valid base64-encoded string.");
            }

            if (_key.Length != 32)
            {
                throw new InvalidOperationException(
                    "Configuration error: Encryption:Key must decode to exactly 32 bytes (256 bits).");
            }
        }

        public string? Encrypt(string? plaintext)
        {
            if (plaintext is null) return null;
            return Convert.ToBase64String(EncryptBytes(Encoding.UTF8.GetBytes(plaintext))!);
        }

        public string? Decrypt(string? ciphertext)
        {
            if (ciphertext is null) return null;

            byte[] packed;
            try
            {
                packed = Convert.FromBase64String(ciphertext);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(
                    "Encrypted field value is not valid base64 — the data is corrupt, was " +
                    "written before encryption was enabled, or was encrypted with a different key.");
            }

            return Encoding.UTF8.GetString(DecryptBytes(packed)!);
        }

        public byte[]? EncryptBytes(byte[]? plainBytes)
        {
            if (plainBytes is null) return null;

            var nonce = new byte[NonceSizeBytes];
            RandomNumberGenerator.Fill(nonce);
            var cipherBytes = new byte[plainBytes.Length];
            var tag = new byte[TagSizeBytes];

            using (var aes = new AesGcm(_key, TagSizeBytes))
            {
                aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
            }

            var packed = new byte[NonceSizeBytes + cipherBytes.Length + TagSizeBytes];
            Buffer.BlockCopy(nonce, 0, packed, 0, NonceSizeBytes);
            Buffer.BlockCopy(cipherBytes, 0, packed, NonceSizeBytes, cipherBytes.Length);
            Buffer.BlockCopy(tag, 0, packed, NonceSizeBytes + cipherBytes.Length, TagSizeBytes);

            return packed;
        }

        public byte[]? DecryptBytes(byte[]? packedBytes)
        {
            if (packedBytes is null) return null;

            if (packedBytes.Length < NonceSizeBytes + TagSizeBytes)
            {
                throw new InvalidOperationException("Encrypted field value is truncated or corrupt.");
            }

            var nonce = packedBytes.AsSpan(0, NonceSizeBytes);
            var tag = packedBytes.AsSpan(packedBytes.Length - TagSizeBytes, TagSizeBytes);
            var cipherBytes = packedBytes.AsSpan(NonceSizeBytes, packedBytes.Length - NonceSizeBytes - TagSizeBytes);
            var plainBytes = new byte[cipherBytes.Length];

            using (var aes = new AesGcm(_key, TagSizeBytes))
            {
                aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
            }

            return plainBytes;
        }
    }
}

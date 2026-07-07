using CoparentHub.Domain.Common;
using System.Security.Cryptography;

namespace CoparentHub.Domain.Entities
{
    public class PasswordResetToken : BaseEntity
    {
        private static readonly TimeSpan Validity = TimeSpan.FromHours(1);

        public Guid UserId { get; private set; }
        public string Token { get; private set; } = default!;
        public DateTime ExpiresAt { get; private set; }
        public bool Used { get; private set; }

        private PasswordResetToken() { }

        public static PasswordResetToken Create(Guid userId) => new()
        {
            UserId = userId,
            Token = GenerateToken(),
            ExpiresAt = DateTime.UtcNow.Add(Validity),
            Used = false,
        };

        public bool IsValid => !Used && DateTime.UtcNow < ExpiresAt;

        public void MarkUsed() => Used = true;

        private static string GenerateToken()
        {
            Span<byte> bytes = stackalloc byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}

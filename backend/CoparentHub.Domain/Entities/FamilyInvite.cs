using CoparentHub.Domain.Common;
using System.Security.Cryptography;

namespace CoparentHub.Domain.Entities
{
    public class FamilyInvite : BaseEntity
    {
        private const int CodeLength = 8;
        private static readonly TimeSpan Validity = TimeSpan.FromHours(48);

        private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

        public Guid FamilyId { get; private set; }
        public string Code { get; private set; } = default!;
        public DateTime ExpiresAt { get; private set; }
        public bool Used { get; private set; }

        public string? InviteeEmail { get; private set; }

        private FamilyInvite() { }

        public static FamilyInvite Create(Guid familyId) => new()
        {
            FamilyId = familyId,
            Code = GenerateCode(),
            ExpiresAt = DateTime.UtcNow.Add(Validity),
            Used = false,
        };

        public bool IsValid => !Used && DateTime.UtcNow < ExpiresAt;

        public void MarkUsed() => Used = true;

        public void SetInviteeEmail(string email) => InviteeEmail = email.Trim().ToLower();

        private static string GenerateCode()
        {
            Span<char> chars = stackalloc char[CodeLength];
            for (var i = 0; i < CodeLength; i++)
                chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
            return new string(chars);
        }
    }
}

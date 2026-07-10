using CoparentHub.Domain.Common;

namespace CoparentHub.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; private set; } = default!;
        public string LastName { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;

        // Bumped on password change; invalidates outstanding JWTs (see JwtBearerEvents).
        public Guid SecurityStamp { get; private set; } = Guid.NewGuid();
        public int FailedLoginCount { get; private set; }
        public DateTime? LockedUntil { get; private set; }

        private User() { }

        public static User Create(string firstName, string lastName, string email, string passwordHash)
        {
            return new User
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email.Trim().ToLower(),
                PasswordHash = passwordHash,
            };
        }

        public string FullName => $"{FirstName} {LastName}";
    }
}

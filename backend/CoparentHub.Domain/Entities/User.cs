using CoparentHub.Domain.Common;

namespace CoparentHub.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; private set; } = default!;
        public string LastName { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;

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

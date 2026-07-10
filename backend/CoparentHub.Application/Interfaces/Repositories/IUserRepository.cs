using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsAsync(string email, CancellationToken ct = default);
        void Add(User user);
        void Remove(User user);
        Task SetPasswordHashAsync(Guid userId, string passwordHash, CancellationToken ct = default);
        Task RecordFailedLoginAsync(Guid userId, int lockoutThreshold, TimeSpan lockoutDuration, CancellationToken ct = default);
        Task ResetFailedLoginAsync(Guid userId, CancellationToken ct = default);
    }

}

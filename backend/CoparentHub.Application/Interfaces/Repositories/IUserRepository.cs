using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsAsync(string email, CancellationToken ct = default);
        void Add(User user);
    }

}

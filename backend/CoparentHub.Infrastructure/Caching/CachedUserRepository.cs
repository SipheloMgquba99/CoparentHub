using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace CoparentHub.Infrastructure.Caching
{
    public class CachedUserRepository(IUserRepository inner, IMemoryCache cache) : IUserRepository
    {
        private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);
        private static string Key(Guid id) => $"user:{id}";

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            if (cache.TryGetValue(Key(id), out User? cached)) return cached;

            var user = await inner.GetByIdAsync(id, ct);
            if (user is not null) cache.Set(Key(id), user, Ttl);
            return user;
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
            inner.GetByEmailAsync(email, ct);

        public Task<bool> ExistsAsync(string email, CancellationToken ct = default) =>
            inner.ExistsAsync(email, ct);

        public void Add(User user) => inner.Add(user);

        public async Task SetPasswordHashAsync(Guid userId, string passwordHash, CancellationToken ct = default)
        {
            await inner.SetPasswordHashAsync(userId, passwordHash, ct);
            cache.Remove(Key(userId));
        }
    }
}

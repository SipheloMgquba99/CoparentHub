using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface IPushSubscriptionRepository
    {
        Task<PushSubscription?> GetByEndpointAsync(string endpoint, CancellationToken ct = default);
        Task<List<PushSubscription>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<List<PushSubscription>> GetAllAsync(CancellationToken ct = default);
        void Add(PushSubscription subscription);
        Task RemoveByEndpointAsync(string endpoint, CancellationToken ct = default);
    }
}

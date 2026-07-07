using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Notification>> GetByUserAsync(Guid userId, CancellationToken ct = default);
        void Add(Notification notification);
        Task DeleteAllForFamilyAsync(Guid familyId, CancellationToken ct = default);
    }
}

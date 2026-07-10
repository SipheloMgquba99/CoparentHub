using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface IMessageRepository
    {
        Task<Message?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Message>> GetByFamilyAsync(Guid familyId, CancellationToken ct = default);
        void Add(Message message);
        Task MarkThreadReadAsync(Guid familyId, Guid readerUserId, CancellationToken ct = default);
    }
}

using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface IEventRepository
    {
        Task<ScheduledEvent?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<ScheduledEvent>> GetByFamilyAsync(Guid familyId, DateOnly? from, DateOnly? to, Guid? childId, CancellationToken ct = default);
        Task<List<ScheduledEvent>> GetWeekAsync(Guid familyId, DateOnly weekStart, CancellationToken ct = default);
        void Add(ScheduledEvent ev);
        Task DeleteAllForFamilyAsync(Guid familyId, CancellationToken ct = default);
        Task<List<ScheduledEvent>> GetStartingSoonAsync(DateTime notBefore, DateTime notAfter, CancellationToken ct = default);
    }
}

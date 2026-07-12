using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface ICustodyScheduleRepository
    {
        Task<CustodySchedule?> GetActiveByFamilyIdAsync(Guid familyId, CancellationToken ct = default);
        Task<CustodySchedule?> GetByIdAsync(Guid id, CancellationToken ct = default);
        void Add(CustodySchedule schedule);
        Task DeactivateAllForFamilyAsync(Guid familyId, CancellationToken ct = default);
    }
}

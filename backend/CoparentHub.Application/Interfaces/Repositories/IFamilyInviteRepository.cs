using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface IFamilyInviteRepository
    {
        Task<FamilyInvite?> GetActiveByFamilyIdAsync(Guid familyId, CancellationToken ct = default);
        Task<FamilyInvite?> GetLatestUnusedByFamilyIdAsync(Guid familyId, CancellationToken ct = default);
        Task<FamilyInvite?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<FamilyInvite?> GetActiveByEmailAsync(string email, CancellationToken ct = default);
        void Add(FamilyInvite invite);
    }
}

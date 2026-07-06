using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface IFamilyInviteRepository
    {
        Task<FamilyInvite?> GetActiveByFamilyIdAsync(Guid familyId, CancellationToken ct = default);
        Task<FamilyInvite?> GetByCodeAsync(string code, CancellationToken ct = default);
        void Add(FamilyInvite invite);
    }
}

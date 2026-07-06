using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface IFamilyRepository
    {
        Task<Family?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Family>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        void Add(Family family);
    }
}

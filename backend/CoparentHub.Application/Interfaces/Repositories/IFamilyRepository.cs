using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface IFamilyRepository
    {
        Task<Family?> GetByIdAsync(Guid id, CancellationToken ct = default);
        void Add(Family family);
    }
}

using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface IExpenseRepository
    {
        Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Expense>> GetByFamilyAsync(Guid familyId, CancellationToken ct = default);
        void Add(Expense expense);
        void Remove(Expense expense);
        Task MarkAllSettledAsync(Guid familyId, CancellationToken ct = default);
        Task DeleteAllForFamilyAsync(Guid familyId, CancellationToken ct = default);
    }
}

using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Entities;
using CoparentHub.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace CoparentHub.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext db) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
            db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);
        public Task<bool> ExistsAsync(string email, CancellationToken ct = default) =>
            db.Users.AnyAsync(u => u.Email == email.ToLower(), ct);
        public void Add(User user) => db.Users.Add(user);
    }

    public class FamilyRepository(AppDbContext db) : IFamilyRepository
    {
        public Task<Family?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
         db.Families
           .Include(f => f.Members)
           .Include(f => f.Children)
           .FirstOrDefaultAsync(f => f.Id == id, ct);
        public void Add(Family family) => db.Families.Add(family);
    }

    public class EventRepository(AppDbContext db) : IEventRepository
    {
        public Task<ScheduledEvent?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            db.Events.Include(e => e.Attendances).FirstOrDefaultAsync(e => e.Id == id, ct);

        public async Task<List<ScheduledEvent>> GetByFamilyAsync(
            Guid familyId, DateOnly? from, DateOnly? to, Guid? childId, CancellationToken ct = default)
        {
            var q = db.Events.Include(e => e.Attendances).Where(e => e.FamilyId == familyId);
            if (from.HasValue)
            {
                var fromDt = DateTime.SpecifyKind(from.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                q = q.Where(e => e.StartsAt >= fromDt);
            }
            if (to.HasValue)
            {
                var toDt = DateTime.SpecifyKind(to.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
                q = q.Where(e => e.StartsAt <= toDt);
            }
            if (childId.HasValue) q = q.Where(e => e.ChildId == childId.Value);
            return await q.OrderBy(e => e.StartsAt).ToListAsync(ct);
        }

        public Task<List<ScheduledEvent>> GetWeekAsync(Guid familyId, DateOnly weekStart, CancellationToken ct = default)
        {
            var start = DateTime.SpecifyKind(weekStart.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(weekStart.AddDays(7).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            return db.Events.Include(e => e.Attendances)
                .Where(e => e.FamilyId == familyId && e.StartsAt >= start && e.StartsAt < end && !e.IsCancelled)
                .OrderBy(e => e.StartsAt).ToListAsync(ct);
        }

        public void Add(ScheduledEvent ev) => db.Events.Add(ev);
    }

    public class UnitOfWork(AppDbContext db, IUserRepository users, IFamilyRepository families, IEventRepository events)
    : IUnitOfWork
    {
        public IUserRepository Users { get; } = users;
        public IFamilyRepository Families { get; } = families;
        public IEventRepository Events { get; } = events;
        public Task SaveAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
    }
}

using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Entities;
using CoparentHub.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace CoparentHub.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext db) : IUserRepository
    {
        // AsNoTracking is safe here: nothing in the app mutates a User after RegisterHandler
        // creates it (no profile-edit feature exists), so these reads never need to be tracked
        // for a later SaveChanges, and detached instances are safe to cache across requests.
        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
            db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);
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

        public async Task<List<Family>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            var familyIds = await db.Members
                .Where(m => m.UserId == userId)
                .Select(m => m.FamilyId)
                .ToListAsync(ct);

            return await db.Families
                .Include(f => f.Members)
                .Include(f => f.Children)
                .Where(f => familyIds.Contains(f.Id))
                .ToListAsync(ct);
        }

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

    public class NotificationRepository(AppDbContext db) : INotificationRepository
    {
        public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);

        public Task<List<Notification>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
            db.Notifications.Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(100)
                .ToListAsync(ct);

        public void Add(Notification notification) => db.Notifications.Add(notification);
    }

    public class FamilyInviteRepository(AppDbContext db) : IFamilyInviteRepository
    {
        public Task<FamilyInvite?> GetActiveByFamilyIdAsync(Guid familyId, CancellationToken ct = default) =>
            db.FamilyInvites
                .Where(i => i.FamilyId == familyId && !i.Used && i.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(i => i.ExpiresAt)
                .FirstOrDefaultAsync(ct);

        public Task<FamilyInvite?> GetLatestUnusedByFamilyIdAsync(Guid familyId, CancellationToken ct = default) =>
            db.FamilyInvites
                .Where(i => i.FamilyId == familyId && !i.Used)
                .OrderByDescending(i => i.ExpiresAt)
                .FirstOrDefaultAsync(ct);

        public Task<FamilyInvite?> GetByCodeAsync(string code, CancellationToken ct = default) =>
            db.FamilyInvites.FirstOrDefaultAsync(i => i.Code == code, ct);

        public void Add(FamilyInvite invite) => db.FamilyInvites.Add(invite);
    }

    public class UnitOfWork(
        AppDbContext db,
        IUserRepository users,
        IFamilyRepository families,
        IEventRepository events,
        INotificationRepository notifications,
        IFamilyInviteRepository invites)
    : IUnitOfWork
    {
        public IUserRepository Users { get; } = users;
        public IFamilyRepository Families { get; } = families;
        public IEventRepository Events { get; } = events;
        public INotificationRepository Notifications { get; } = notifications;
        public IFamilyInviteRepository Invites { get; } = invites;
        public Task SaveAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
    }
}

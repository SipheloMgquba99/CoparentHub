using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Entities;
using CoparentHub.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace CoparentHub.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext db) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
            db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);
        public Task<bool> ExistsAsync(string email, CancellationToken ct = default) =>
            db.Users.AnyAsync(u => u.Email == email.ToLower(), ct);
        public void Add(User user) => db.Users.Add(user);
        public void Remove(User user) => db.Users.Remove(user);

        public async Task SetPasswordHashAsync(Guid userId, string passwordHash, CancellationToken ct = default) =>
            await db.Users.Where(u => u.Id == userId).ExecuteUpdateAsync(s => s
                .SetProperty(u => u.PasswordHash, passwordHash)
                .SetProperty(u => u.SecurityStamp, Guid.NewGuid()), ct);

        public async Task RecordFailedLoginAsync(Guid userId, int lockoutThreshold, TimeSpan lockoutDuration, CancellationToken ct = default) =>
            await db.Users.Where(u => u.Id == userId).ExecuteUpdateAsync(s => s
                .SetProperty(u => u.FailedLoginCount, u => u.FailedLoginCount + 1)
                .SetProperty(u => u.LockedUntil, u =>
                    u.FailedLoginCount + 1 >= lockoutThreshold
                        ? DateTime.UtcNow.Add(lockoutDuration)
                        : u.LockedUntil), ct);

        public async Task ResetFailedLoginAsync(Guid userId, CancellationToken ct = default) =>
            await db.Users.Where(u => u.Id == userId).ExecuteUpdateAsync(s => s
                .SetProperty(u => u.FailedLoginCount, 0)
                .SetProperty(u => u.LockedUntil, (DateTime?)null), ct);
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
        public void Remove(Family family) => db.Families.Remove(family);
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

        public Task<List<ScheduledEvent>> GetStartingSoonAsync(DateTime notBefore, DateTime notAfter, CancellationToken ct = default) =>
            db.Events.Include(e => e.Attendances)
                .Where(e => !e.IsCancelled && !e.ReminderSent && e.StartsAt > notBefore && e.StartsAt <= notAfter)
                .ToListAsync(ct);
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

        public Task<FamilyInvite?> GetActiveByEmailAsync(string email, CancellationToken ct = default) =>
            db.FamilyInvites
                .Where(i => i.InviteeEmail == email.Trim().ToLower() && !i.Used && i.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(i => i.ExpiresAt)
                .FirstOrDefaultAsync(ct);

        public void Add(FamilyInvite invite) => db.FamilyInvites.Add(invite);
    }

    public class PasswordResetTokenRepository(AppDbContext db) : IPasswordResetTokenRepository
    {
        public Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken ct = default) =>
            db.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token, ct);

        public void Add(PasswordResetToken token) => db.PasswordResetTokens.Add(token);
    }

    public class PushSubscriptionRepository(AppDbContext db) : IPushSubscriptionRepository
    {
        public Task<PushSubscription?> GetByEndpointAsync(string endpoint, CancellationToken ct = default) =>
            db.PushSubscriptions.FirstOrDefaultAsync(p => p.Endpoint == endpoint, ct);

        public Task<List<PushSubscription>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
            db.PushSubscriptions.Where(p => p.UserId == userId).ToListAsync(ct);

        public Task<List<PushSubscription>> GetAllAsync(CancellationToken ct = default) =>
            db.PushSubscriptions.ToListAsync(ct);

        public void Add(PushSubscription subscription) => db.PushSubscriptions.Add(subscription);

        public async Task RemoveByEndpointAsync(string endpoint, CancellationToken ct = default) =>
            await db.PushSubscriptions.Where(p => p.Endpoint == endpoint).ExecuteDeleteAsync(ct);
    }

    public class ExpenseRepository(AppDbContext db) : IExpenseRepository
    {
        public Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            db.Expenses.FirstOrDefaultAsync(e => e.Id == id, ct);

        public Task<List<Expense>> GetByFamilyAsync(Guid familyId, CancellationToken ct = default) =>
            db.Expenses.Where(e => e.FamilyId == familyId).ToListAsync(ct);

        public void Add(Expense expense) => db.Expenses.Add(expense);
        public void Remove(Expense expense) => db.Expenses.Remove(expense);

        public async Task MarkAllSettledAsync(Guid familyId, CancellationToken ct = default) =>
            await db.Expenses.Where(e => e.FamilyId == familyId && !e.IsSettled)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsSettled, true), ct);
    }

    public class MessageRepository(AppDbContext db) : IMessageRepository
    {
        public Task<Message?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            db.Messages.FirstOrDefaultAsync(m => m.Id == id, ct);

        public Task<List<Message>> GetByFamilyAsync(Guid familyId, CancellationToken ct = default) =>
            db.Messages.Where(m => m.FamilyId == familyId)
                .OrderBy(m => m.CreatedAt)
                .Take(500)
                .ToListAsync(ct);

        public void Add(Message message) => db.Messages.Add(message);

        public async Task MarkThreadReadAsync(Guid familyId, Guid readerUserId, CancellationToken ct = default) =>
            await db.Messages
                .Where(m => m.FamilyId == familyId && m.SenderUserId != readerUserId && !m.IsReadByRecipient)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsReadByRecipient, true), ct);
    }

    public class DocumentRepository(AppDbContext db) : IDocumentRepository
    {
        public Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);

        public Task<List<DocumentSummary>> GetSummariesByFamilyAsync(Guid familyId, CancellationToken ct = default) =>
            db.Documents
                .Where(d => d.FamilyId == familyId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DocumentSummary(
                    d.Id, d.FamilyId, d.ChildId, d.UploadedByUserId,
                    d.FileName, d.ContentType, d.Category, d.SizeBytes, d.Description, d.CreatedAt))
                .ToListAsync(ct);

        public void Add(Document document) => db.Documents.Add(document);
        public void Remove(Document document) => db.Documents.Remove(document);
    }

    public class UnitOfWork(
        AppDbContext db,
        IUserRepository users,
        IFamilyRepository families,
        IEventRepository events,
        INotificationRepository notifications,
        IFamilyInviteRepository invites,
        IPasswordResetTokenRepository passwordResetTokens,
        IPushSubscriptionRepository pushSubscriptions,
        IExpenseRepository expenses,
        IMessageRepository messages,
        IDocumentRepository documents)
    : IUnitOfWork
    {
        public IUserRepository Users { get; } = users;
        public IFamilyRepository Families { get; } = families;
        public IEventRepository Events { get; } = events;
        public INotificationRepository Notifications { get; } = notifications;
        public IFamilyInviteRepository Invites { get; } = invites;
        public IPasswordResetTokenRepository PasswordResetTokens { get; } = passwordResetTokens;
        public IPushSubscriptionRepository PushSubscriptions { get; } = pushSubscriptions;
        public IExpenseRepository Expenses { get; } = expenses;
        public IMessageRepository Messages { get; } = messages;
        public IDocumentRepository Documents { get; } = documents;
        public Task SaveAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);

        // If EnableRetryOnFailure is ever added to the Npgsql context, this must switch to
        // db.Database.CreateExecutionStrategy().ExecuteAsync(...) or EF Core will throw here.
        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            try
            {
                await operation(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }
}

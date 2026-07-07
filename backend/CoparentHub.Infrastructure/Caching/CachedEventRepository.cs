using CoparentHub.Application.Interfaces;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace CoparentHub.Infrastructure.Caching
{
    /// <summary>
    /// Caches the two list-reads (GetByFamilyAsync/GetWeekAsync) that back the polled
    /// events/weekly-schedule endpoints. GetByIdAsync — the single-entity read used by every
    /// mutation handler (update/cancel/rsvp) — deliberately passes straight through: it needs
    /// a fresh, currently-tracked entity to save changes against, so it must never be served
    /// from here. Reads are invalidated via <see cref="IEventCacheVersion"/>, bumped by any
    /// handler that creates, updates, cancels, or RSVPs to an event for the family — a short
    /// TTL alone isn't enough, since a user's own immediate refetch right after their own
    /// mutation would otherwise see a stale cached response.
    /// </summary>
    public class CachedEventRepository(IEventRepository inner, IMemoryCache cache, IEventCacheVersion version) : IEventRepository
    {
        private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

        public Task<ScheduledEvent?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            inner.GetByIdAsync(id, ct);

        public async Task<List<ScheduledEvent>> GetByFamilyAsync(
            Guid familyId, DateOnly? from, DateOnly? to, Guid? childId, CancellationToken ct = default)
        {
            var key = $"events:{familyId}:v{version.Current(familyId)}:{from}:{to}:{childId}";
            if (cache.TryGetValue(key, out List<ScheduledEvent>? cached) && cached is not null) return cached;

            var result = await inner.GetByFamilyAsync(familyId, from, to, childId, ct);
            cache.Set(key, result, Ttl);
            return result;
        }

        public async Task<List<ScheduledEvent>> GetWeekAsync(Guid familyId, DateOnly weekStart, CancellationToken ct = default)
        {
            var key = $"week:{familyId}:v{version.Current(familyId)}:{weekStart}";
            if (cache.TryGetValue(key, out List<ScheduledEvent>? cached) && cached is not null) return cached;

            var result = await inner.GetWeekAsync(familyId, weekStart, ct);
            cache.Set(key, result, Ttl);
            return result;
        }

        public void Add(ScheduledEvent ev) => inner.Add(ev);

        public Task DeleteAllForFamilyAsync(Guid familyId, CancellationToken ct = default) =>
            inner.DeleteAllForFamilyAsync(familyId, ct);
        public Task<List<ScheduledEvent>> GetStartingSoonAsync(DateTime notBefore, DateTime notAfter, CancellationToken ct = default) =>
            inner.GetStartingSoonAsync(notBefore, notAfter, ct);
    }
}

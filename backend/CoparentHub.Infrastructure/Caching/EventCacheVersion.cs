using CoparentHub.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CoparentHub.Infrastructure.Caching
{
    public class EventCacheVersion(IMemoryCache cache) : IEventCacheVersion
    {
        private static string Key(Guid familyId) => $"event-cache-version:{familyId}";

        public int Current(Guid familyId) =>
            cache.TryGetValue(Key(familyId), out int v) ? v : 0;

        public void Bump(Guid familyId) =>
            cache.Set(Key(familyId), Current(familyId) + 1);
    }
}

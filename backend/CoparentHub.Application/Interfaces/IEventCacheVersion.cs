namespace CoparentHub.Application.Interfaces
{
    /// <summary>
    /// Tracks a per-family version number used to invalidate cached event/weekly-schedule
    /// reads. Any handler that creates, updates, cancels, or RSVPs to an event must call
    /// <see cref="Bump"/> for that event's family after a successful save.
    /// </summary>
    public interface IEventCacheVersion
    {
        int Current(Guid familyId);
        void Bump(Guid familyId);
    }
}

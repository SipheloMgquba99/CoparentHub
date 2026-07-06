using CoparentHub.Domain.Common;

namespace CoparentHub.Domain.Entities
{
    public enum EventType { School, Medical,Activity, Other }
    public enum AttendanceStatus { Accepted, Tentative, Declined }

    public class Attendance : BaseEntity
    {
        public Guid EventId { get; private set; }
        public Guid UserId { get; private set; }
        public AttendanceStatus Status { get; private set; }
        public DateTime? RespondedAt { get; private set; }
        public string? Reason { get; private set; }

        private Attendance() { }

        internal static Attendance For(Guid eventId, Guid userId, bool attending) => new()
        {
            EventId = eventId,
            UserId = userId,
            Status = attending ? AttendanceStatus.Accepted : AttendanceStatus.Tentative,
            RespondedAt = attending ? DateTime.UtcNow : null,
        };

        internal void Update(AttendanceStatus status, string? reason)
        {
            Status = status;
            RespondedAt = DateTime.UtcNow;
            Reason = status == AttendanceStatus.Declined ? reason?.Trim() : null;
        }
    }

    public class ScheduledEvent : BaseEntity
    {
        public Guid FamilyId { get; private set; }
        public Guid ChildId { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public string Title { get; private set; } = default!;
        public string? Notes { get; private set; }
        public EventType Type { get; private set; }
        public DateTime StartsAt { get; private set; }
        public DateTime? EndsAt { get; private set; }
        public bool IsCancelled { get; private set; }

        private readonly List<Attendance> _attendances = [];
        public IReadOnlyList<Attendance> Attendances => _attendances.AsReadOnly();

        private ScheduledEvent() { }

        public static ScheduledEvent Create(
            Guid familyId, Guid childId, Guid createdBy,
            string title, EventType type, DateTime startsAt, DateTime? endsAt, string? notes,
            IEnumerable<Guid> memberIds)
        {
            var ev = new ScheduledEvent
            {
                FamilyId = familyId,
                ChildId = childId,
                CreatedByUserId = createdBy,
                Title = title.Trim(),
                Type = type,
                // Normalize stored times to UTC kind
                StartsAt = DateTime.SpecifyKind(startsAt, DateTimeKind.Utc),
                EndsAt = endsAt.HasValue ? DateTime.SpecifyKind(endsAt.Value, DateTimeKind.Utc) : null,
                Notes = notes?.Trim(),
            };

            foreach (var memberId in memberIds)
                ev._attendances.Add(Attendance.For(ev.Id, memberId, attending: memberId == createdBy));

            return ev;
        }

        public Result<ScheduledEvent> Update(string title, EventType type, DateTime startsAt, DateTime? endsAt, string? notes)
        {
            if (IsCancelled) return Result<ScheduledEvent>.Fail("Cannot edit a cancelled event.");
            Title = title.Trim();
            Type = type;
            // Normalize on update as well
            StartsAt = DateTime.SpecifyKind(startsAt, DateTimeKind.Utc);
            EndsAt = endsAt.HasValue ? DateTime.SpecifyKind(endsAt.Value, DateTimeKind.Utc) : null;
            Notes = notes?.Trim();
            return Result<ScheduledEvent>.Ok(this);
        }

        public Result<ScheduledEvent> Cancel()
        {
            if (IsCancelled) return Result<ScheduledEvent>.Fail("Event is already cancelled.");
            IsCancelled = true;
            return Result<ScheduledEvent>.Ok(this);
        }

        public Result<Attendance> Rsvp(Guid userId, AttendanceStatus status, string? reason)
        {
            if (IsCancelled) return Result<Attendance>.Fail("Cannot RSVP to a cancelled event.");
            if (status == AttendanceStatus.Declined && string.IsNullOrWhiteSpace(reason))
                return Result<Attendance>.Fail("A reason is required when declining.");
            var attendance = _attendances.FirstOrDefault(a => a.UserId == userId);
            if (attendance is null) return Result<Attendance>.Fail("User is not part of this event.");
            attendance.Update(status, reason);
            return Result<Attendance>.Ok(attendance);
        }
    }
}

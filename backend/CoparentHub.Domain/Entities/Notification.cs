using CoparentHub.Domain.Common;

namespace CoparentHub.Domain.Entities
{
    public enum NotificationType
    {
        EventRsvp,
        EventCreated,
        EventCancelled,
        EventReminder
    }

    public class Notification : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid FamilyId { get; private set; }
        public NotificationType Type { get; private set; }
        public string Message { get; private set; } = default!;
        public Guid? EventId { get; private set; }
        public bool IsRead { get; private set; }

        private Notification() { }

        public static Notification Create(
            Guid userId, Guid familyId, NotificationType type, string message, Guid? eventId)
            => new()
            {
                UserId = userId,
                FamilyId = familyId,
                Type = type,
                Message = message,
                EventId = eventId,
                IsRead = false,
            };

        public void MarkRead() => IsRead = true;
    }
}

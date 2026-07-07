using CoparentHub.Domain.Common;

namespace CoparentHub.Domain.Entities
{
    public class PushSubscription : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Endpoint { get; private set; } = default!;
        public string P256dh { get; private set; } = default!;
        public string Auth { get; private set; } = default!;

        private PushSubscription() { }

        public static PushSubscription Create(Guid userId, string endpoint, string p256dh, string auth) => new()
        {
            UserId = userId,
            Endpoint = endpoint,
            P256dh = p256dh,
            Auth = auth,
        };
    }
}

namespace CoparentHub.Application.Interfaces
{
    public enum PushSendOutcome
    {
        Sent,
        SubscriptionExpired,
        Failed
    }

    public interface IPushSender
    {
        /// <summary>
        /// True when VAPID keys are configured. When false, callers should skip sending
        /// entirely rather than attempt it (see <see cref="NullPushSender"/> in
        /// CoparentHub.Infrastructure).
        /// </summary>
        bool IsConfigured { get; }

        Task<PushSendOutcome> SendAsync(
            string endpoint,
            string p256dh,
            string auth,
            string payloadJson,
            CancellationToken ct = default);
    }
}

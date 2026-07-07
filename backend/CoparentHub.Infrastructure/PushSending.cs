using CoparentHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using WebPush;
using LibPushSubscription = WebPush.PushSubscription;

namespace CoparentHub.Infrastructure
{
    public class WebPushSender(IConfiguration config, ILogger<WebPushSender> logger) : IPushSender
    {
        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(config["Vapid:PublicKey"]) &&
            !string.IsNullOrWhiteSpace(config["Vapid:PrivateKey"]) &&
            !string.IsNullOrWhiteSpace(config["Vapid:Subject"]);

        public async Task<PushSendOutcome> SendAsync(
            string endpoint, string p256dh, string auth, string payloadJson, CancellationToken ct = default)
        {
            var vapidDetails = new VapidDetails(
                config["Vapid:Subject"], config["Vapid:PublicKey"], config["Vapid:PrivateKey"]);
            var subscription = new LibPushSubscription(endpoint, p256dh, auth);

            try
            {
                // A fresh client per call is cheap and sidesteps any thread-safety questions
                // about reusing one instance across concurrent sends in the reminder fan-out.
                var client = new WebPushClient();
                await client.SendNotificationAsync(subscription, payloadJson, vapidDetails, cancellationToken: ct);
                return PushSendOutcome.Sent;
            }
            catch (WebPushException ex) when (
                ex.StatusCode == HttpStatusCode.NotFound || ex.StatusCode == HttpStatusCode.Gone)
            {
                logger.LogInformation(
                    "Push subscription expired (status={StatusCode}), will be pruned: {Endpoint}",
                    ex.StatusCode, endpoint);
                return PushSendOutcome.SubscriptionExpired;
            }
            catch (WebPushException ex)
            {
                logger.LogWarning(ex, "Push send failed (status={StatusCode}) for {Endpoint}", ex.StatusCode, endpoint);
                return PushSendOutcome.Failed;
            }
        }
    }

    /// <summary>
    /// Registered when no VAPID keys are configured (e.g. local dev). Never sends — callers
    /// must check <see cref="IsConfigured"/> and skip sending, so a call reaching here is
    /// unexpected.
    /// </summary>
    public class NullPushSender(ILogger<NullPushSender> logger) : IPushSender
    {
        public bool IsConfigured => false;

        public Task<PushSendOutcome> SendAsync(
            string endpoint, string p256dh, string auth, string payloadJson, CancellationToken ct = default)
        {
            logger.LogWarning("Push send attempted with no VAPID keys configured (endpoint={Endpoint})", endpoint);
            return Task.FromResult(PushSendOutcome.Failed);
        }
    }
}

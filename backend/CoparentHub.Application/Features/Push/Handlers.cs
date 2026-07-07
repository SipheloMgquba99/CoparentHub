using CoparentHub.Application.Interfaces;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace CoparentHub.Application.Features.Push
{
    public class SubscribePushHandler(IUnitOfWork uow) : IRequestHandler<SubscribePushCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(SubscribePushCommand cmd, CancellationToken ct)
        {
            var existing = await uow.PushSubscriptions.GetByEndpointAsync(cmd.Endpoint, ct);

            if (existing is not null && existing.UserId == cmd.UserId)
                return Result<Guid>.Ok(existing.Id);

            if (existing is not null)
            {
                await uow.PushSubscriptions.RemoveByEndpointAsync(cmd.Endpoint, ct);
            }

            var subscription = PushSubscription.Create(cmd.UserId, cmd.Endpoint, cmd.P256dh, cmd.Auth);
            uow.PushSubscriptions.Add(subscription);
            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(subscription.Id);
        }
    }

    public class UnsubscribePushHandler(IUnitOfWork uow) : IRequestHandler<UnsubscribePushCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UnsubscribePushCommand cmd, CancellationToken ct)
        {
            var existing = await uow.PushSubscriptions.GetByEndpointAsync(cmd.Endpoint, ct);

            if (existing is null)
                return Result<bool>.Ok(true);

            if (existing.UserId != cmd.UserId)
                return Result<bool>.Fail("Access denied.");

            await uow.PushSubscriptions.RemoveByEndpointAsync(cmd.Endpoint, ct);
            return Result<bool>.Ok(true);
        }
    }

    public class GetVapidPublicKeyHandler(IConfiguration config) : IRequestHandler<GetVapidPublicKeyQuery, Result<string>>
    {
        public Task<Result<string>> Handle(GetVapidPublicKeyQuery q, CancellationToken ct)
        {
            var key = config["Vapid:PublicKey"];

            return Task.FromResult(string.IsNullOrWhiteSpace(key)
                ? Result<string>.Fail("Push notifications aren't available in this environment.")
                : Result<string>.Ok(key));
        }
    }

    public class SendDueEventRemindersHandler(IUnitOfWork uow, IPushSender pushSender, IConfiguration config)
        : IRequestHandler<SendDueEventRemindersCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(SendDueEventRemindersCommand cmd, CancellationToken ct)
        {
            var windowMinutes = config.GetValue("Reminders:WindowMinutes", 60);
            var now = DateTime.UtcNow;
            var events = await uow.Events.GetStartingSoonAsync(now, now.AddMinutes(windowMinutes), ct);

            var remindedCount = 0;

            foreach (var ev in events)
            {
                var family = await uow.Families.GetByIdAsync(ev.FamilyId, ct);
                if (family is null) continue;

                var childName = family.Children.FirstOrDefault(c => c.Id == ev.ChildId)?.Name ?? "your child";
                var recipientIds = ev.Attendances
                    .Where(a => a.Status != AttendanceStatus.Declined)
                    .Select(a => a.UserId)
                    .ToList();
                var message = $"\"{ev.Title}\" for {childName} starts soon.";

                foreach (var userId in recipientIds)
                {
                    uow.Notifications.Add(Notification.Create(
                        userId, ev.FamilyId, NotificationType.EventReminder, message, ev.Id));
                }

                if (pushSender.IsConfigured)
                {
                    var payload = JsonSerializer.Serialize(new { title = "Upcoming event", body = message, url = "/" });
                    await PushFanout.SendToUsersAsync(uow, pushSender, recipientIds, payload, ct);
                }

                ev.MarkReminderSent();
                remindedCount++;
            }

            await uow.SaveAsync(ct);
            return Result<int>.Ok(remindedCount);
        }
    }

    public class SendAnnouncementHandler(IUnitOfWork uow, IPushSender pushSender, IConfiguration config)
        : IRequestHandler<SendAnnouncementCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(SendAnnouncementCommand cmd, CancellationToken ct)
        {
            var adminEmail = config["Admin:Email"];

            if (string.IsNullOrWhiteSpace(adminEmail) ||
                !string.Equals(cmd.SenderEmail, adminEmail, StringComparison.OrdinalIgnoreCase))
                return Result<int>.Fail("Access denied.");

            if (!pushSender.IsConfigured)
                return Result<int>.Ok(0);

            var subscriptions = await uow.PushSubscriptions.GetAllAsync(ct);
            var payload = JsonSerializer.Serialize(new { title = cmd.Title, body = cmd.Body, url = cmd.Url ?? "/" });
            var sentCount = await PushFanout.SendToSubscriptionsAsync(uow, pushSender, subscriptions, payload, ct);

            return Result<int>.Ok(sentCount);
        }
    }

    internal static class PushFanout
    {
        public static async Task<int> SendToUsersAsync(
            IUnitOfWork uow, IPushSender pushSender, IEnumerable<Guid> userIds, string payloadJson, CancellationToken ct)
        {
            var sentCount = 0;

            foreach (var userId in userIds)
            {
                var subscriptions = await uow.PushSubscriptions.GetByUserIdAsync(userId, ct);
                sentCount += await SendToSubscriptionsAsync(uow, pushSender, subscriptions, payloadJson, ct);
            }

            return sentCount;
        }

        public static async Task<int> SendToSubscriptionsAsync(
            IUnitOfWork uow, IPushSender pushSender, IEnumerable<PushSubscription> subscriptions,
            string payloadJson, CancellationToken ct)
        {
            var sentCount = 0;

            foreach (var sub in subscriptions)
            {
                var outcome = await pushSender.SendAsync(sub.Endpoint, sub.P256dh, sub.Auth, payloadJson, ct);

                if (outcome == PushSendOutcome.Sent)
                    sentCount++;
                else if (outcome == PushSendOutcome.SubscriptionExpired)
                    await uow.PushSubscriptions.RemoveByEndpointAsync(sub.Endpoint, ct);
            }

            return sentCount;
        }
    }
}

using CoparentHub.Application.Features.DTOs;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Notifications
{
    public class GetNotificationsHandler(IUnitOfWork uow)
        : IRequestHandler<GetNotificationsQuery, Result<List<NotificationDto>>>
    {
        public async Task<Result<List<NotificationDto>>> Handle(GetNotificationsQuery q, CancellationToken ct)
        {
            var notifications = await uow.Notifications.GetByUserAsync(q.UserId, ct);

            return Result<List<NotificationDto>>.Ok(
                notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new NotificationDto(
                        n.Id, n.Type.ToString(), n.Message, n.EventId, n.IsRead, n.CreatedAt))
                    .ToList());
        }
    }

    public class MarkNotificationReadHandler(IUnitOfWork uow)
        : IRequestHandler<MarkNotificationReadCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(MarkNotificationReadCommand cmd, CancellationToken ct)
        {
            var notification = await uow.Notifications.GetByIdAsync(cmd.NotificationId, ct);

            if (notification is null)
                return Result<Guid>.Fail("Notification not found.");

            if (notification.UserId != cmd.UserId)
                return Result<Guid>.Fail("Access denied.");

            notification.MarkRead();

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(notification.Id);
        }
    }
}

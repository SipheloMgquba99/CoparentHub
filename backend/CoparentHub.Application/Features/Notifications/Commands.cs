using CoparentHub.Application.Features.DTOs;
using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Notifications
{
    public record GetNotificationsQuery(Guid UserId)
        : IRequest<Result<List<NotificationDto>>>;

    public record MarkNotificationReadCommand(Guid NotificationId, Guid UserId)
        : IRequest<Result<Guid>>;
}

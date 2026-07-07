using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Push
{
    public record SubscribePushCommand(
        Guid UserId,
        string Endpoint,
        string P256dh,
        string Auth
    ) : IRequest<Result<Guid>>;

    public record UnsubscribePushCommand(
        Guid UserId,
        string Endpoint
    ) : IRequest<Result<bool>>;

    public record SendDueEventRemindersCommand() : IRequest<Result<int>>;

    public record SendAnnouncementCommand(
        string SenderEmail,
        string Title,
        string Body,
        string? Url
    ) : IRequest<Result<int>>;
}

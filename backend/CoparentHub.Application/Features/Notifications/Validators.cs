using FluentValidation;

namespace CoparentHub.Application.Features.Notifications
{
    public class GetNotificationsValidator : AbstractValidator<GetNotificationsQuery>
    {
        public GetNotificationsValidator()
        {
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class MarkNotificationReadValidator : AbstractValidator<MarkNotificationReadCommand>
    {
        public MarkNotificationReadValidator()
        {
            RuleFor(x => x.NotificationId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }
}

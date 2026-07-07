using FluentValidation;

namespace CoparentHub.Application.Features.Push
{
    public class SubscribePushValidator : AbstractValidator<SubscribePushCommand>
    {
        public SubscribePushValidator()
        {
            RuleFor(x => x.Endpoint).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.P256dh).NotEmpty().MaximumLength(255);
            RuleFor(x => x.Auth).NotEmpty().MaximumLength(255);
        }
    }

    public class UnsubscribePushValidator : AbstractValidator<UnsubscribePushCommand>
    {
        public UnsubscribePushValidator()
        {
            RuleFor(x => x.Endpoint).NotEmpty().MaximumLength(1000);
        }
    }

    public class SendAnnouncementValidator : AbstractValidator<SendAnnouncementCommand>
    {
        public SendAnnouncementValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Body).NotEmpty().MaximumLength(500);
        }
    }
}

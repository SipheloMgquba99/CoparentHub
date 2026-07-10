using FluentValidation;

namespace CoparentHub.Application.Features.Messages
{
    public class SendMessageValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);

            RuleFor(x => x.Body)
                .NotEmpty()
                .MaximumLength(2000);
        }
    }

    public class MarkThreadReadValidator : AbstractValidator<MarkThreadReadCommand>
    {
        public MarkThreadReadValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class GetMessagesValidator : AbstractValidator<GetMessagesQuery>
    {
        public GetMessagesValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }
}

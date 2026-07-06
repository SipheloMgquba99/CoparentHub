using FluentValidation;

namespace CoparentHub.Application.Features.Events
{
    public class CreateEventValidator : AbstractValidator<CreateEventCommand>
    {
        public CreateEventValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.EndsAt)
                .GreaterThan(x => x.StartsAt)
                .When(x => x.EndsAt.HasValue);

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .When(x => x.Notes != null);
        }
    }

    public class UpdateEventValidator : AbstractValidator<UpdateEventCommand>
    {
        public UpdateEventValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.EndsAt)
                .GreaterThan(x => x.StartsAt)
                .When(x => x.EndsAt.HasValue);
        }
    }

    public class RsvpValidator : AbstractValidator<RsvpCommand>
    {
        public RsvpValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("A reason is required when declining.")
                .When(x => x.Status == Domain.Entities.AttendanceStatus.Declined);

            RuleFor(x => x.Reason)
                .MaximumLength(200);
        }
    }
}

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
}

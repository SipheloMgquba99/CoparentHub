using FluentValidation;

namespace CoparentHub.Application.Features.Events
{
    public class CreateEventValidator : AbstractValidator<CreateEventCommand>
    {
        public CreateEventValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.ChildId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Type).IsInEnum();

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
            RuleFor(x => x.EventId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Type).IsInEnum();

            RuleFor(x => x.EndsAt)
                .GreaterThan(x => x.StartsAt)
                .When(x => x.EndsAt.HasValue);
        }
    }

    public class CancelEventValidator : AbstractValidator<CancelEventCommand>
    {
        public CancelEventValidator()
        {
            RuleFor(x => x.EventId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class RsvpValidator : AbstractValidator<RsvpCommand>
    {
        public RsvpValidator()
        {
            RuleFor(x => x.EventId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);

            RuleFor(x => x.Status).IsInEnum();

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("A reason is required when declining.")
                .When(x => x.Status == Domain.Entities.AttendanceStatus.Declined);

            RuleFor(x => x.Reason)
                .MaximumLength(200);
        }
    }

    public class GetEventsValidator : AbstractValidator<GetEventsQuery>
    {
        public GetEventsValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
            RuleFor(x => x.ChildId).NotEqual(Guid.Empty).When(x => x.ChildId.HasValue);

            RuleFor(x => x)
                .Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
                .WithMessage("From must be on or before To.");
        }
    }

    public class GetWeeklyValidator : AbstractValidator<GetWeeklyQuery>
    {
        public GetWeeklyValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }
}

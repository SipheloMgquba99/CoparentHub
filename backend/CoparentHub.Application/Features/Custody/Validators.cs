using FluentValidation;

namespace CoparentHub.Application.Features.Custody
{
    public class CreateCustodyScheduleValidator : AbstractValidator<CreateCustodyScheduleCommand>
    {
        public CreateCustodyScheduleValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
            RuleFor(x => x.ParentAUserId).NotEqual(Guid.Empty);
            RuleFor(x => x.ParentBUserId).NotEqual(Guid.Empty);

            RuleFor(x => x.CycleLengthDays).InclusiveBetween(1, 90);

            RuleFor(x => x.DayPattern)
                .NotEmpty()
                .Must((cmd, pattern) => pattern.Length == cmd.CycleLengthDays)
                    .WithMessage("DayPattern length must match CycleLengthDays.")
                .Must(pattern => pattern.All(c => c is 'A' or 'B'))
                    .WithMessage("DayPattern must contain only 'A' or 'B'.");

            RuleFor(x => x)
                .Must(x => x.ParentAUserId != x.ParentBUserId)
                .WithMessage("ParentA and ParentB must be different people.");
        }
    }

    public class GetActiveCustodyScheduleValidator : AbstractValidator<GetActiveCustodyScheduleQuery>
    {
        public GetActiveCustodyScheduleValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class GetCustodyForRangeValidator : AbstractValidator<GetCustodyForRangeQuery>
    {
        public GetCustodyForRangeValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);

            RuleFor(x => x)
                .Must(x => x.From <= x.To)
                .WithMessage("From must be on or before To.");

            RuleFor(x => x)
                .Must(x => x.To.DayNumber - x.From.DayNumber <= 62)
                .WithMessage("Range cannot exceed 62 days.");
        }
    }
}

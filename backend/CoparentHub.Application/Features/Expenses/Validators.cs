using FluentValidation;

namespace CoparentHub.Application.Features.Expenses
{
    public class CreateExpenseValidator : AbstractValidator<CreateExpenseCommand>
    {
        public CreateExpenseValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .LessThanOrEqualTo(1_000_000);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(300);

            RuleFor(x => x.SplitPercentForPayer)
                .InclusiveBetween(0, 100);

            RuleFor(x => x.Category)
                .IsInEnum();

            RuleFor(x => x.Date)
                .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Date cannot be in the future.");
        }
    }
}

using FluentValidation;

namespace CoparentHub.Application.Features.Expenses
{
    public class CreateExpenseValidator : AbstractValidator<CreateExpenseCommand>
    {
        public CreateExpenseValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
            RuleFor(x => x.ChildId).NotEqual(Guid.Empty).When(x => x.ChildId.HasValue);

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

    public class RemoveExpenseValidator : AbstractValidator<RemoveExpenseCommand>
    {
        public RemoveExpenseValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.ExpenseId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class MarkAllExpensesSettledValidator : AbstractValidator<MarkAllExpensesSettledCommand>
    {
        public MarkAllExpensesSettledValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class GetExpensesValidator : AbstractValidator<GetExpensesQuery>
    {
        public GetExpensesValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class GetExpenseBalanceValidator : AbstractValidator<GetExpenseBalanceQuery>
    {
        public GetExpenseBalanceValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }
}

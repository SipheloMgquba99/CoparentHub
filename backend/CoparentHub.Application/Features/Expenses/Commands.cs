using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using MediatR;

namespace CoparentHub.Application.Features.Expenses
{
    public record CreateExpenseCommand(
        Guid FamilyId,
        Guid UserId,
        Guid? ChildId,
        decimal Amount,
        string Description,
        ExpenseCategory Category,
        DateOnly Date,
        decimal SplitPercentForPayer
    ) : IRequest<Result<Guid>>;

    public record RemoveExpenseCommand(
        Guid FamilyId,
        Guid ExpenseId,
        Guid UserId
    ) : IRequest<Result<Guid>>;

    public record MarkAllExpensesSettledCommand(
        Guid FamilyId,
        Guid UserId
    ) : IRequest<Result<Guid>>;
}

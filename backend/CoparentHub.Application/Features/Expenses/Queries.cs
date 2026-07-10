using CoparentHub.Application.Features.DTOs;
using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Expenses
{
    public record GetExpensesQuery(Guid FamilyId, Guid UserId) : IRequest<Result<List<ExpenseDto>>>;

    public record GetExpenseBalanceQuery(Guid FamilyId, Guid UserId) : IRequest<Result<ExpenseBalanceDto>>;
}

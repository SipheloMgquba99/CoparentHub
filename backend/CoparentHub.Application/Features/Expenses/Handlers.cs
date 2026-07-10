using CoparentHub.Application.Features.DTOs;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using MediatR;

namespace CoparentHub.Application.Features.Expenses
{
    public class CreateExpenseHandler(IUnitOfWork uow)
        : IRequestHandler<CreateExpenseCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateExpenseCommand cmd, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(cmd.FamilyId, ct);

            if (family is null)
                return Result<Guid>.Fail("Family not found.");

            if (!family.IsMember(cmd.UserId))
                return Result<Guid>.Fail("Access denied.");

            if (cmd.ChildId.HasValue && !family.HasChild(cmd.ChildId.Value))
                return Result<Guid>.Fail("Child not found in this family.");

            var expense = Expense.Create(
                cmd.FamilyId, cmd.ChildId, cmd.UserId,
                cmd.Amount, cmd.Description, cmd.Category,
                cmd.Date, cmd.SplitPercentForPayer);

            uow.Expenses.Add(expense);

            var payer = await uow.Users.GetByIdAsync(cmd.UserId, ct);
            var payerName = payer?.FullName ?? "Someone";

            foreach (var member in family.Members.Where(m => m.UserId != cmd.UserId))
            {
                uow.Notifications.Add(Notification.Create(
                    userId: member.UserId,
                    familyId: cmd.FamilyId,
                    type: NotificationType.ExpenseAdded,
                    message: $"{payerName} added an expense: {cmd.Description} (R{cmd.Amount:0.00}).",
                    eventId: null));
            }

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(expense.Id);
        }
    }

    public class RemoveExpenseHandler(IUnitOfWork uow)
        : IRequestHandler<RemoveExpenseCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RemoveExpenseCommand cmd, CancellationToken ct)
        {
            var expense = await uow.Expenses.GetByIdAsync(cmd.ExpenseId, ct);

            if (expense is null || expense.FamilyId != cmd.FamilyId)
                return Result<Guid>.Fail("Expense not found.");

            if (expense.PaidByUserId != cmd.UserId)
                return Result<Guid>.Fail("Only the person who paid can remove this expense.");

            if (expense.IsSettled)
                return Result<Guid>.Fail("Cannot remove an expense that has already been settled.");

            uow.Expenses.Remove(expense);
            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(cmd.ExpenseId);
        }
    }

    public class MarkAllExpensesSettledHandler(IUnitOfWork uow)
        : IRequestHandler<MarkAllExpensesSettledCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(MarkAllExpensesSettledCommand cmd, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(cmd.FamilyId, ct);

            if (family is null)
                return Result<Guid>.Fail("Family not found.");

            if (!family.IsMember(cmd.UserId))
                return Result<Guid>.Fail("Access denied.");

            await uow.Expenses.MarkAllSettledAsync(cmd.FamilyId, ct);

            return Result<Guid>.Ok(cmd.FamilyId);
        }
    }

    public class GetExpensesHandler(IUnitOfWork uow)
        : IRequestHandler<GetExpensesQuery, Result<List<ExpenseDto>>>
    {
        public async Task<Result<List<ExpenseDto>>> Handle(GetExpensesQuery q, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(q.FamilyId, ct);

            if (family is null)
                return Result<List<ExpenseDto>>.Fail("Family not found.");

            if (!family.IsMember(q.UserId))
                return Result<List<ExpenseDto>>.Fail("Access denied.");

            var expenses = await uow.Expenses.GetByFamilyAsync(q.FamilyId, ct);

            var userNames = new Dictionary<Guid, string>();
            foreach (var userId in expenses.Select(e => e.PaidByUserId).Distinct())
            {
                var user = await uow.Users.GetByIdAsync(userId, ct);
                userNames[userId] = user?.FullName ?? "Unknown";
            }

            var dtos = expenses
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedAt)
                .Select(e => new ExpenseDto(
                    e.Id,
                    e.FamilyId,
                    e.ChildId,
                    e.ChildId.HasValue ? family.Children.FirstOrDefault(c => c.Id == e.ChildId)?.Name : null,
                    e.PaidByUserId,
                    userNames.GetValueOrDefault(e.PaidByUserId, "Unknown"),
                    e.Amount,
                    e.Description,
                    e.Category.ToString(),
                    e.Date,
                    e.SplitPercentForPayer,
                    e.IsSettled,
                    e.CreatedAt))
                .ToList();

            return Result<List<ExpenseDto>>.Ok(dtos);
        }
    }

    public class GetExpenseBalanceHandler(IUnitOfWork uow)
        : IRequestHandler<GetExpenseBalanceQuery, Result<ExpenseBalanceDto>>
    {
        public async Task<Result<ExpenseBalanceDto>> Handle(GetExpenseBalanceQuery q, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(q.FamilyId, ct);

            if (family is null)
                return Result<ExpenseBalanceDto>.Fail("Family not found.");

            if (!family.IsMember(q.UserId))
                return Result<ExpenseBalanceDto>.Fail("Access denied.");

            if (family.Members.Count < 2)
                return Result<ExpenseBalanceDto>.Ok(new ExpenseBalanceDto(q.FamilyId, null, null, 0m));

            var memberA = family.Members[0].UserId;
            var memberB = family.Members[1].UserId;

            var expenses = await uow.Expenses.GetByFamilyAsync(q.FamilyId, ct);

            // Positive => memberB owes memberA; negative => memberA owes memberB.
            decimal net = 0;
            foreach (var e in expenses.Where(e => !e.IsSettled))
            {
                var owedToPayer = Math.Round(e.Amount * (100 - e.SplitPercentForPayer) / 100m, 2, MidpointRounding.AwayFromZero);
                if (e.PaidByUserId == memberA) net += owedToPayer;
                else if (e.PaidByUserId == memberB) net -= owedToPayer;
            }

            net = Math.Round(net, 2, MidpointRounding.AwayFromZero);

            if (net == 0)
                return Result<ExpenseBalanceDto>.Ok(new ExpenseBalanceDto(q.FamilyId, null, null, 0m));

            return net > 0
                ? Result<ExpenseBalanceDto>.Ok(new ExpenseBalanceDto(q.FamilyId, memberB, memberA, net))
                : Result<ExpenseBalanceDto>.Ok(new ExpenseBalanceDto(q.FamilyId, memberA, memberB, -net));
        }
    }
}

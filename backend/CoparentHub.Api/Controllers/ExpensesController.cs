using CoparentHub.Application.Features.Expenses;
using CoparentHub.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoparentHub.Api.Controllers
{
    [Route("api/families/{familyId:guid}/expenses")]
    [Authorize]
    public class ExpensesController(ISender sender) : ApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new GetExpensesQuery(familyId, CurrentUserId), ct));

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new GetExpenseBalanceQuery(familyId, CurrentUserId), ct));

        [HttpPost]
        public async Task<IActionResult> Create(Guid familyId, [FromBody] CreateExpenseRequest req, CancellationToken ct)
            => ToResponse(await sender.Send(
                new CreateExpenseCommand(
                    familyId,
                    CurrentUserId,
                    req.ChildId,
                    req.Amount,
                    req.Description,
                    req.Category,
                    req.Date,
                    req.SplitPercentForPayer), ct));

        [HttpDelete("{expenseId:guid}")]
        public async Task<IActionResult> Remove(Guid familyId, Guid expenseId, CancellationToken ct)
            => ToResponse(await sender.Send(new RemoveExpenseCommand(familyId, expenseId, CurrentUserId), ct));

        [HttpPost("settle-all")]
        public async Task<IActionResult> SettleAll(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new MarkAllExpensesSettledCommand(familyId, CurrentUserId), ct));
    }

    public record CreateExpenseRequest(
        Guid? ChildId,
        decimal Amount,
        string Description,
        ExpenseCategory Category,
        DateOnly Date,
        decimal SplitPercentForPayer);
}

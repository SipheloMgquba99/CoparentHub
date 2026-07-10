namespace CoparentHub.Application.Features.DTOs
{
    public record ExpenseDto(
        Guid Id,
        Guid FamilyId,
        Guid? ChildId,
        string? ChildName,
        Guid PaidByUserId,
        string PaidByName,
        decimal Amount,
        string Description,
        string Category,
        DateOnly Date,
        decimal SplitPercentForPayer,
        bool IsSettled,
        DateTime CreatedAt);

    public record ExpenseBalanceDto(
        Guid FamilyId,
        Guid? OwedByUserId,
        Guid? OwedToUserId,
        decimal Amount);
}

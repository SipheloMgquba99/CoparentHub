using CoparentHub.Domain.Common;

namespace CoparentHub.Domain.Entities
{
    public enum ExpenseCategory { Medical, School, Clothing, Activity, Childcare, Other }

    public class Expense : BaseEntity
    {
        public Guid FamilyId { get; private set; }
        public Guid? ChildId { get; private set; }
        public Guid PaidByUserId { get; private set; }
        public decimal Amount { get; private set; }
        public string Description { get; private set; } = default!;
        public ExpenseCategory Category { get; private set; }
        public DateOnly Date { get; private set; }
        public decimal SplitPercentForPayer { get; private set; }
        public bool IsSettled { get; private set; }

        private Expense() { }

        public static Expense Create(
            Guid familyId, Guid? childId, Guid paidByUserId,
            decimal amount, string description, ExpenseCategory category,
            DateOnly date, decimal splitPercentForPayer) => new()
        {
            FamilyId = familyId,
            ChildId = childId,
            PaidByUserId = paidByUserId,
            Amount = amount,
            Description = description.Trim(),
            Category = category,
            Date = date,
            SplitPercentForPayer = splitPercentForPayer,
            IsSettled = false,
        };
    }
}

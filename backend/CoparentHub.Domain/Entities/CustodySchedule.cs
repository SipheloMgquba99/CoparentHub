using CoparentHub.Domain.Common;

namespace CoparentHub.Domain.Entities
{
    public class CustodySchedule : BaseEntity
    {
        public Guid FamilyId { get; private set; }
        public DateOnly StartDate { get; private set; }
        public int CycleLengthDays { get; private set; }
        public string DayPattern { get; private set; } = default!;
        public Guid ParentAUserId { get; private set; }
        public Guid ParentBUserId { get; private set; }
        public bool IsActive { get; private set; }

        private CustodySchedule() { }

        public static CustodySchedule Create(
            Guid familyId, DateOnly startDate, int cycleLengthDays, string dayPattern,
            Guid parentAUserId, Guid parentBUserId) => new()
        {
            FamilyId = familyId,
            StartDate = startDate,
            CycleLengthDays = cycleLengthDays,
            DayPattern = dayPattern,
            ParentAUserId = parentAUserId,
            ParentBUserId = parentBUserId,
            IsActive = true,
        };
    }
}

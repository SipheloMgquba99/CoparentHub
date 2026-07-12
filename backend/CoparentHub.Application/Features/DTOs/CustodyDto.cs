namespace CoparentHub.Application.Features.DTOs
{
    public record CustodyScheduleDto(
        Guid Id,
        DateOnly StartDate,
        int CycleLengthDays,
        string DayPattern,
        Guid ParentAUserId,
        string ParentAName,
        Guid ParentBUserId,
        string ParentBName);

    public record CustodyDayDto(
        DateOnly Date,
        string DayName,
        Guid ParentUserId,
        string ParentName);

    public record CustodyRangeDto(
        DateOnly From,
        DateOnly To,
        List<CustodyDayDto> Days);
}

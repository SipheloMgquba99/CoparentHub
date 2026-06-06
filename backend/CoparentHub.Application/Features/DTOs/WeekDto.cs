namespace CoparentHub.Application.Features.DTOs
{
    public record WeekDto(DateOnly WeekStart, DateOnly WeekEnd, List<DayDto> Days);
}

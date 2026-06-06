namespace CoparentHub.Application.Features.DTOs
{
    public record DayDto(DateOnly Date, string DayName, List<EventDto> Events);
}

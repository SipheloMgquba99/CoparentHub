namespace CoparentHub.Application.Features.DTOs
{
    public record AttendanceDto(Guid UserId, string Status, DateTime? RespondedAt, string? Reason);
}

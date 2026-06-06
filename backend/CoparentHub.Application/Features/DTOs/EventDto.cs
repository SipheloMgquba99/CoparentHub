namespace CoparentHub.Application.Features.DTOs
{
    public record EventDto(
      Guid Id, Guid FamilyId, Guid ChildId, string ChildName,
      string Title, string Type, DateTime StartsAt, DateTime? EndsAt,
      string? Notes, bool IsCancelled, List<AttendanceDto> Attendances);
}

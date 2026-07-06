namespace CoparentHub.Application.Features.DTOs
{
    public record NotificationDto(
        Guid Id, string Type, string Message, Guid? EventId, bool IsRead, DateTime CreatedAt);
}

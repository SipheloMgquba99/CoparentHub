namespace CoparentHub.Application.Features.DTOs
{
    public record MessageDto(
        Guid Id,
        Guid FamilyId,
        Guid SenderUserId,
        string SenderName,
        string Body,
        bool IsReadByRecipient,
        DateTime CreatedAt);
}

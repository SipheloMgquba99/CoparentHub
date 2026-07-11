namespace CoparentHub.Application.Features.DTOs
{
    public record DocumentDto(
        Guid Id,
        Guid FamilyId,
        Guid? ChildId,
        string? ChildName,
        Guid UploadedByUserId,
        string UploadedByName,
        string FileName,
        string ContentType,
        string Category,
        long SizeBytes,
        string? Description,
        DateTime CreatedAt);

    public record DocumentContentDto(byte[] Content, string ContentType, string FileName);
}

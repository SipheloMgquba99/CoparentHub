using CoparentHub.Domain.Entities;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public record DocumentSummary(
        Guid Id,
        Guid FamilyId,
        Guid? ChildId,
        Guid UploadedByUserId,
        string FileName,
        string ContentType,
        DocumentCategory Category,
        long SizeBytes,
        string? Description,
        DateTime CreatedAt);

    public interface IDocumentRepository
    {
        Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<DocumentSummary>> GetSummariesByFamilyAsync(Guid familyId, CancellationToken ct = default);
        void Add(Document document);
        void Remove(Document document);
    }
}

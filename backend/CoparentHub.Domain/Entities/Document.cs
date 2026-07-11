using CoparentHub.Domain.Common;

namespace CoparentHub.Domain.Entities
{
    public enum DocumentCategory { Legal, Medical, School, Financial, Other }

    public class Document : BaseEntity
    {
        public Guid FamilyId { get; private set; }
        public Guid? ChildId { get; private set; }
        public Guid UploadedByUserId { get; private set; }
        public string FileName { get; private set; } = default!;
        public string ContentType { get; private set; } = default!;
        public DocumentCategory Category { get; private set; }
        public long SizeBytes { get; private set; }
        public byte[] Content { get; private set; } = default!;
        public string? Description { get; private set; }

        private Document() { }

        public static Document Create(
            Guid familyId, Guid? childId, Guid uploadedByUserId,
            string fileName, string contentType, DocumentCategory category,
            byte[] content, string? description) => new()
        {
            FamilyId = familyId,
            ChildId = childId,
            UploadedByUserId = uploadedByUserId,
            FileName = fileName.Trim(),
            ContentType = contentType,
            Category = category,
            SizeBytes = content.Length,
            Content = content,
            Description = description?.Trim(),
        };
    }
}

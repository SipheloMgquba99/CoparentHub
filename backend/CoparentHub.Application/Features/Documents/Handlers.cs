using CoparentHub.Application.Features.DTOs;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using MediatR;

namespace CoparentHub.Application.Features.Documents
{
    public class UploadDocumentHandler(IUnitOfWork uow, IFieldEncryptor encryptor)
        : IRequestHandler<UploadDocumentCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(UploadDocumentCommand cmd, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(cmd.FamilyId, ct);

            if (family is null)
                return Result<Guid>.Fail("Family not found.");

            if (!family.IsMember(cmd.UserId))
                return Result<Guid>.Fail("Access denied.");

            if (cmd.ChildId.HasValue && !family.HasChild(cmd.ChildId.Value))
                return Result<Guid>.Fail("Child not found in this family.");

            var encryptedContent = encryptor.EncryptBytes(cmd.Content)!;

            var document = Document.Create(
                cmd.FamilyId, cmd.ChildId, cmd.UserId,
                cmd.FileName, cmd.ContentType, cmd.Category,
                encryptedContent, cmd.Description);

            uow.Documents.Add(document);

            var uploader = await uow.Users.GetByIdAsync(cmd.UserId, ct);
            var uploaderName = uploader?.FullName ?? "Someone";

            foreach (var member in family.Members.Where(m => m.UserId != cmd.UserId))
            {
                uow.Notifications.Add(Notification.Create(
                    userId: member.UserId,
                    familyId: cmd.FamilyId,
                    type: NotificationType.DocumentUploaded,
                    message: $"{uploaderName} uploaded a document: {cmd.FileName}.",
                    eventId: null));
            }

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(document.Id);
        }
    }

    public class RemoveDocumentHandler(IUnitOfWork uow)
        : IRequestHandler<RemoveDocumentCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RemoveDocumentCommand cmd, CancellationToken ct)
        {
            var document = await uow.Documents.GetByIdAsync(cmd.DocumentId, ct);

            if (document is null || document.FamilyId != cmd.FamilyId)
                return Result<Guid>.Fail("Document not found.");

            var family = await uow.Families.GetByIdAsync(cmd.FamilyId, ct);

            if (family is null || !family.IsMember(cmd.UserId))
                return Result<Guid>.Fail("Access denied.");

            uow.Documents.Remove(document);
            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(cmd.DocumentId);
        }
    }

    public class GetDocumentsHandler(IUnitOfWork uow)
        : IRequestHandler<GetDocumentsQuery, Result<List<DocumentDto>>>
    {
        public async Task<Result<List<DocumentDto>>> Handle(GetDocumentsQuery q, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(q.FamilyId, ct);

            if (family is null)
                return Result<List<DocumentDto>>.Fail("Family not found.");

            if (!family.IsMember(q.UserId))
                return Result<List<DocumentDto>>.Fail("Access denied.");

            var summaries = await uow.Documents.GetSummariesByFamilyAsync(q.FamilyId, ct);

            var userNames = new Dictionary<Guid, string>();
            foreach (var userId in summaries.Select(s => s.UploadedByUserId).Distinct())
            {
                var user = await uow.Users.GetByIdAsync(userId, ct);
                userNames[userId] = user?.FullName ?? "Unknown";
            }

            var dtos = summaries
                .Select(s => new DocumentDto(
                    s.Id,
                    s.FamilyId,
                    s.ChildId,
                    s.ChildId.HasValue ? family.Children.FirstOrDefault(c => c.Id == s.ChildId)?.Name : null,
                    s.UploadedByUserId,
                    userNames.GetValueOrDefault(s.UploadedByUserId, "Unknown"),
                    s.FileName,
                    s.ContentType,
                    s.Category.ToString(),
                    s.SizeBytes,
                    s.Description,
                    s.CreatedAt))
                .ToList();

            return Result<List<DocumentDto>>.Ok(dtos);
        }
    }

    public class GetDocumentContentHandler(IUnitOfWork uow, IFieldEncryptor encryptor)
        : IRequestHandler<GetDocumentContentQuery, Result<DocumentContentDto>>
    {
        public async Task<Result<DocumentContentDto>> Handle(GetDocumentContentQuery q, CancellationToken ct)
        {
            var document = await uow.Documents.GetByIdAsync(q.DocumentId, ct);

            if (document is null || document.FamilyId != q.FamilyId)
                return Result<DocumentContentDto>.Fail("Document not found.");

            var family = await uow.Families.GetByIdAsync(q.FamilyId, ct);

            if (family is null || !family.IsMember(q.UserId))
                return Result<DocumentContentDto>.Fail("Access denied.");

            var decryptedContent = encryptor.DecryptBytes(document.Content)!;

            return Result<DocumentContentDto>.Ok(
                new DocumentContentDto(decryptedContent, document.ContentType, document.FileName));
        }
    }
}

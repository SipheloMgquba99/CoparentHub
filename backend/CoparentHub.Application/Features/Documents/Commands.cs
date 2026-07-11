using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using MediatR;

namespace CoparentHub.Application.Features.Documents
{
    public record UploadDocumentCommand(
        Guid FamilyId,
        Guid UserId,
        Guid? ChildId,
        string FileName,
        string ContentType,
        byte[] Content,
        DocumentCategory Category,
        string? Description
    ) : IRequest<Result<Guid>>;

    public record RemoveDocumentCommand(
        Guid FamilyId,
        Guid DocumentId,
        Guid UserId
    ) : IRequest<Result<Guid>>;
}

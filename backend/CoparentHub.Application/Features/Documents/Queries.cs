using CoparentHub.Application.Features.DTOs;
using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Documents
{
    public record GetDocumentsQuery(Guid FamilyId, Guid UserId) : IRequest<Result<List<DocumentDto>>>;

    public record GetDocumentContentQuery(Guid FamilyId, Guid DocumentId, Guid UserId) : IRequest<Result<DocumentContentDto>>;
}

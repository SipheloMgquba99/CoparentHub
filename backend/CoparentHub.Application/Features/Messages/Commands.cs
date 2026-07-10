using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Messages
{
    public record SendMessageCommand(
        Guid FamilyId,
        Guid UserId,
        string Body
    ) : IRequest<Result<Guid>>;

    public record MarkThreadReadCommand(
        Guid FamilyId,
        Guid UserId
    ) : IRequest<Result<bool>>;
}

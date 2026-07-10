using CoparentHub.Application.Features.DTOs;
using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Messages
{
    public record GetMessagesQuery(Guid FamilyId, Guid UserId) : IRequest<Result<List<MessageDto>>>;
}

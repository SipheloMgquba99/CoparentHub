using CoparentHub.Application.Features.DTOs;
using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Family
{
    public record GetFamilyQuery(
    Guid FamilyId,
    Guid UserId
    ) : IRequest<Result<FamilyDto>>;
}

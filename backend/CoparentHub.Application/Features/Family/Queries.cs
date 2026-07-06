using CoparentHub.Application.Features.DTOs;
using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Family
{
    public record GetFamilyQuery(
    Guid FamilyId,
    Guid UserId
    ) : IRequest<Result<FamilyDto>>;

    public record GetMyFamiliesQuery(
        Guid UserId
    ) : IRequest<Result<List<FamilyDto>>>;
}

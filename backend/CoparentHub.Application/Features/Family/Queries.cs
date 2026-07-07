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

    public record GetActiveFamilyInviteQuery(
        Guid FamilyId,
        Guid UserId
    ) : IRequest<Result<FamilyInviteDto?>>;

    public record GetFamilyInviteStatusQuery(
        Guid FamilyId,
        Guid UserId
    ) : IRequest<Result<FamilyInviteStatusDto?>>;

    public record GetPendingInviteQuery(
        string Email
    ) : IRequest<Result<PendingInviteDto?>>;
}

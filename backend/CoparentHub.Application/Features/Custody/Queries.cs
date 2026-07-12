using CoparentHub.Application.Features.DTOs;
using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Custody
{
    public record GetActiveCustodyScheduleQuery(Guid FamilyId, Guid UserId) : IRequest<Result<CustodyScheduleDto>>;

    public record GetCustodyForRangeQuery(
        Guid FamilyId,
        Guid UserId,
        DateOnly From,
        DateOnly To
    ) : IRequest<Result<CustodyRangeDto>>;
}

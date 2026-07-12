using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Custody
{
    public record CreateCustodyScheduleCommand(
        Guid FamilyId,
        Guid UserId,
        DateOnly StartDate,
        int CycleLengthDays,
        string DayPattern,
        Guid ParentAUserId,
        Guid ParentBUserId
    ) : IRequest<Result<Guid>>;
}

using CoparentHub.Application.Features.DTOs;
using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Events
{

    public record GetEventsQuery(
        Guid FamilyId,
        Guid UserId,
        DateTime? From,
        DateTime? To,
        Guid? ChildId
    ) : IRequest<Result<List<EventDto>>>;

    public record GetWeeklyQuery(
        Guid FamilyId,
        Guid UserId,
        DateOnly WeekStart
    ) : IRequest<Result<WeekDto>>;
}

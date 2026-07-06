using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using MediatR;

namespace CoparentHub.Application.Features.Events
{
    public record CreateEventCommand(
     Guid FamilyId,
     Guid ChildId,
     Guid UserId,
     string Title,
     EventType Type,
     DateTime StartsAt,
     DateTime? EndsAt,
     string? Notes
 ) : IRequest<Result<Guid>>;

    public record UpdateEventCommand(
        Guid EventId,
        Guid UserId,
        string Title,
        EventType Type,
        DateTime StartsAt,
        DateTime? EndsAt,
        string? Notes
    ) : IRequest<Result<Guid>>;

    public record CancelEventCommand(Guid EventId, Guid UserId) : IRequest<Result<Guid>>;

    public record RsvpCommand(Guid EventId, Guid UserId, AttendanceStatus Status, string? Reason)
        : IRequest<Result<Guid>>;
}

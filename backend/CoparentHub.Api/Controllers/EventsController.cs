using CoparentHub.Application.Features.Events;
using CoparentHub.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoparentHub.Api.Controllers
{
    [Route("api/families/{familyId:guid}/events")]
    [Authorize]
    public class EventsController(ISender sender) : ApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(
            Guid familyId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] Guid? childId,
            CancellationToken ct)
            => ToResponse(await sender.Send(
                new GetEventsQuery(familyId, CurrentUserId, from, to, childId), ct));

        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeekly(
            Guid familyId,
            [FromQuery] DateOnly weekStart,
            CancellationToken ct)
            => ToResponse(await sender.Send(
                new GetWeeklyQuery(familyId, CurrentUserId, weekStart), ct));

        [HttpPost]
        public async Task<IActionResult> Create(
            Guid familyId,
            [FromBody] CreateEventRequest req,
            CancellationToken ct)
            => ToResponse(await sender.Send(
                new CreateEventCommand(
                    familyId,
                    req.ChildId,
                    CurrentUserId,
                    req.Title,
                    req.Type,
                    req.StartsAt,
                    req.EndsAt,
                    req.Notes), ct));

        [HttpPut("{eventId:guid}")]
        public async Task<IActionResult> Update(
            Guid eventId,
            [FromBody] UpdateEventRequest req,
            CancellationToken ct)
            => ToResponse(await sender.Send(
                new UpdateEventCommand(
                    eventId,
                    CurrentUserId,
                    req.Title,
                    req.Type,
                    req.StartsAt,
                    req.EndsAt,
                    req.Notes), ct));

        [HttpDelete("{eventId:guid}")]
        public async Task<IActionResult> Cancel(Guid eventId, CancellationToken ct)
            => ToResponse(await sender.Send(
                new CancelEventCommand(eventId, CurrentUserId), ct));

        [HttpPost("{eventId:guid}/rsvp")]
        public async Task<IActionResult> Rsvp(
            Guid eventId,
            [FromBody] RsvpRequest req,
            CancellationToken ct)
            => ToResponse(await sender.Send(
                new RsvpCommand(eventId, CurrentUserId, req.Status, req.Reason), ct));
    }

    public record CreateEventRequest(
        Guid ChildId,
        string Title,
        EventType Type,
        DateTime StartsAt,
        DateTime? EndsAt,
        string? Notes);

    public record UpdateEventRequest(
        string Title,
        EventType Type,
        DateTime StartsAt,
        DateTime? EndsAt,
        string? Notes);

    public record RsvpRequest(AttendanceStatus Status, string? Reason);
}

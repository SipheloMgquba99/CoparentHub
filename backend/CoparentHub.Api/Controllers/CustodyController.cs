using CoparentHub.Application.Features.Custody;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoparentHub.Api.Controllers
{
    [Route("api/families/{familyId:guid}/custody")]
    [Authorize]
    public class CustodyController(ISender sender) : ApiController
    {
        [HttpGet("active")]
        public async Task<IActionResult> GetActive(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new GetActiveCustodyScheduleQuery(familyId, CurrentUserId), ct));

        [HttpGet]
        public async Task<IActionResult> GetRange(Guid familyId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
            => ToResponse(await sender.Send(new GetCustodyForRangeQuery(familyId, CurrentUserId, from, to), ct));

        [HttpPost]
        public async Task<IActionResult> Create(Guid familyId, [FromBody] CreateCustodyScheduleRequest req, CancellationToken ct)
            => ToResponse(await sender.Send(
                new CreateCustodyScheduleCommand(
                    familyId, CurrentUserId, req.StartDate, req.CycleLengthDays, req.DayPattern,
                    req.ParentAUserId, req.ParentBUserId), ct));
    }

    public record CreateCustodyScheduleRequest(
        DateOnly StartDate,
        int CycleLengthDays,
        string DayPattern,
        Guid ParentAUserId,
        Guid ParentBUserId);
}

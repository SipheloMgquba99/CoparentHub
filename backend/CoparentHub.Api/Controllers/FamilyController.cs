using CoparentHub.Application.Features.Family;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoparentHub.Api.Controllers
{
    [Route("api/family")]
    [Authorize]
    public class FamilyController(ISender sender) : ApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetMine(CancellationToken ct)
            => ToResponse(await sender.Send(new GetMyFamiliesQuery(CurrentUserId), ct));

        [HttpGet("{familyId:guid}")]
        public async Task<IActionResult> Get(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new GetFamilyQuery(familyId, CurrentUserId), ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFamilyCommand cmd, CancellationToken ct)
            => ToResponse(await sender.Send(cmd with { UserId = CurrentUserId }, ct));

        [HttpPost("{familyId:guid}/join")]
        public async Task<IActionResult> Join(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new JoinFamilyCommand(familyId, CurrentUserId), ct));

        [HttpPost("{familyId:guid}/children")]
        public async Task<IActionResult> AddChild(Guid familyId, [FromBody] AddChildRequest req, CancellationToken ct)
            => ToResponse(await sender.Send(
                new AddChildCommand(familyId, CurrentUserId, req.Name, req.DateOfBirth), ct));

        [HttpDelete("{familyId:guid}/children/{childId:guid}")]
        public async Task<IActionResult> RemoveChild(Guid familyId, Guid childId, CancellationToken ct)
            => ToResponse(await sender.Send(
                new RemoveChildCommand(familyId, childId, CurrentUserId), ct));
    }

    public record AddChildRequest(string Name, DateOnly? DateOfBirth);
}

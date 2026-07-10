using CoparentHub.Application.Features.Messages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoparentHub.Api.Controllers
{
    [Route("api/families/{familyId:guid}/messages")]
    [Authorize]
    public class MessagesController(ISender sender) : ApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new GetMessagesQuery(familyId, CurrentUserId), ct));

        [HttpPost]
        [EnableRateLimiting("messages")]
        public async Task<IActionResult> Send(Guid familyId, [FromBody] SendMessageRequest req, CancellationToken ct)
            => ToResponse(await sender.Send(new SendMessageCommand(familyId, CurrentUserId, req.Body), ct));

        [HttpPost("read")]
        public async Task<IActionResult> MarkRead(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new MarkThreadReadCommand(familyId, CurrentUserId), ct));
    }

    public record SendMessageRequest(string Body);
}

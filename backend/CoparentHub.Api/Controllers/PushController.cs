using CoparentHub.Application.Features.Push;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoparentHub.Api.Controllers
{
    [Route("api/push")]
    [Authorize]
    public class PushController(ISender sender) : ApiController
    {
        [HttpGet("vapid-public-key")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVapidPublicKey(CancellationToken ct)
            => ToResponse(await sender.Send(new GetVapidPublicKeyQuery(), ct));

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribePushRequest req, CancellationToken ct)
            => ToResponse(await sender.Send(
                new SubscribePushCommand(CurrentUserId, req.Endpoint, req.Keys.P256dh, req.Keys.Auth), ct));

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribePushRequest req, CancellationToken ct)
            => ToResponse(await sender.Send(new UnsubscribePushCommand(CurrentUserId, req.Endpoint), ct));

        [HttpPost("announcements")]
        public async Task<IActionResult> SendAnnouncement([FromBody] SendAnnouncementRequest req, CancellationToken ct)
            => ToResponse(await sender.Send(
                new SendAnnouncementCommand(CurrentUserEmail, req.Title, req.Body, req.Url), ct));
    }

    public record PushKeysRequest(string P256dh, string Auth);
    public record SubscribePushRequest(string Endpoint, PushKeysRequest Keys);
    public record UnsubscribePushRequest(string Endpoint);
    public record SendAnnouncementRequest(string Title, string Body, string? Url);
}

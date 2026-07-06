using CoparentHub.Application.Features.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoparentHub.Api.Controllers
{
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController(ISender sender) : ApiController
    {
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct)
            => ToResponse(await sender.Send(new GetNotificationsQuery(CurrentUserId), ct));

        [HttpPost("{notificationId:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid notificationId, CancellationToken ct)
            => ToResponse(await sender.Send(new MarkNotificationReadCommand(notificationId, CurrentUserId), ct));
    }
}

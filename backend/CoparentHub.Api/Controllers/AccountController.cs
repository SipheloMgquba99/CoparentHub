using CoparentHub.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoparentHub.Api.Controllers
{
    [Route("api/account")]
    [Authorize]
    public class AccountController(ISender sender) : ApiController
    {
        [HttpDelete]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Delete([FromBody] DeleteAccountRequest req, CancellationToken ct)
            => ToResponse(await sender.Send(new DeleteAccountCommand(CurrentUserId, req.Password), ct));
    }

    public record DeleteAccountRequest(string Password);
}

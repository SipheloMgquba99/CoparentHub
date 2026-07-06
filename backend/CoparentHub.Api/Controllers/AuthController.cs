using CoparentHub.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoparentHub.Api.Controllers
{
    [Route("api/auth")]
    [EnableRateLimiting("auth")]
    public class AuthController(ISender sender) : ApiController
    {
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterCommand cmd, CancellationToken ct)
            => ToResponse(await sender.Send(cmd, ct));

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginCommand cmd, CancellationToken ct)
            => ToResponse(await sender.Send(cmd, ct));
    }
}

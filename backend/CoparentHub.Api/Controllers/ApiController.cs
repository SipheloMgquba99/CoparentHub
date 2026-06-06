using CoparentHub.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoparentHub.Api.Controllers
{
    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        protected Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")!);

        protected IActionResult ToResponse<T>(Result<T> result)
            => result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new { error = result.Error });
    }
}

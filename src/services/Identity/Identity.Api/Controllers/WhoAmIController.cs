
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthTech.Identity.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhoAmIController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            var userId = User.FindFirst("sub")?.Value;
            var email = User.FindFirst("email")?.Value;
            return Ok(new { userId, email, time = DateTime.UtcNow });
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult Public() => Ok(new { ok = true });
    }
}

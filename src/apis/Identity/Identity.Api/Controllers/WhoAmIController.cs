using HealthTech.BuildingBlocks.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthTech.Identity.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhoAmIController : ControllerBase
    {
        private readonly IRequestContextAccessor _requestContextAccessor;

        public WhoAmIController(IRequestContextAccessor requestContextAccessor)
        {
            _requestContextAccessor = requestContextAccessor;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            var context = _requestContextAccessor.Current;
            return Ok(new
            {
                userId = context.SubjectId,
                email = context.Email,
                activeTenant = context.ActiveMembership is null
                    ? null
                    : new
                    {
                        context.ActiveMembership.TenantId,
                        context.ActiveMembership.TenantName,
                        context.ActiveMembership.Role
                    },
                memberships = context.Memberships.Select(membership => new
                {
                    membership.TenantId,
                    membership.TenantName,
                    membership.Role
                }),
                issuedAt = DateTimeOffset.UtcNow
            });
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult Public() => Ok(new { ok = true });
    }
}

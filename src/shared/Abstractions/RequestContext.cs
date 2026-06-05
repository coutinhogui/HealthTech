using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthTech.BuildingBlocks.Abstractions;

public sealed class TenantAccessOptions
{
    public const string SectionName = "TenantAccess";
    public List<TenantAccessUserOptions> Users { get; init; } = [];
}

public sealed class TenantAccessUserOptions
{
    public string SubjectId { get; init; } = string.Empty;
    public string? Email { get; init; }
    public List<TenantMembershipOptions> Memberships { get; init; } = [];
}

public sealed class TenantMembershipOptions
{
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public string Role { get; init; } = "member";
    public Guid? ProfessionalId { get; init; }
    public bool Enabled { get; init; } = true;
}

public sealed class BootstrapTenantAccessProvider(IOptions<TenantAccessOptions> options) : ITenantAccessProvider
{
    public Task<IReadOnlyCollection<TenantMembership>> GetMembershipsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var subjectId = principal.FindFirstValue("sub")
                        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue("email")
                    ?? principal.FindFirstValue(ClaimTypes.Email);

        var user = options.Value.Users.FirstOrDefault(candidate =>
            (!string.IsNullOrWhiteSpace(subjectId) &&
             string.Equals(candidate.SubjectId, subjectId, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrWhiteSpace(email) &&
             !string.IsNullOrWhiteSpace(candidate.Email) &&
             string.Equals(candidate.Email, email, StringComparison.OrdinalIgnoreCase)));

        IReadOnlyCollection<TenantMembership> memberships = user?.Memberships
            .Where(membership => membership.Enabled)
            .Select(membership => new TenantMembership(
                membership.TenantId,
                string.IsNullOrWhiteSpace(membership.TenantName) ? membership.TenantId.ToString("D") : membership.TenantName,
                string.IsNullOrWhiteSpace(membership.Role) ? "member" : membership.Role,
                membership.ProfessionalId))
            .ToArray() ?? [];

        return Task.FromResult(memberships);
    }
}

public sealed class HttpRequestContextAccessor(IHttpContextAccessor httpContextAccessor) : IRequestContextAccessor
{
    public const string ItemKey = "healthtech.request-context";

    public ResolvedRequestContext Current =>
        httpContextAccessor.HttpContext?.Items[ItemKey] as ResolvedRequestContext
        ?? throw new InvalidOperationException("Request context has not been resolved for this request.");
}

public sealed class RequestContextMiddleware(
    RequestDelegate next,
    ILogger<RequestContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext, ITenantAccessProvider tenantAccessProvider)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var memberships = await tenantAccessProvider.GetMembershipsAsync(httpContext.User, httpContext.RequestAborted);
            var activeMembership = ResolveActiveMembership(httpContext, memberships);
            var context = new ResolvedRequestContext(
                httpContext.User.FindFirstValue("sub")
                    ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException("Authenticated principal is missing the subject identifier."),
                httpContext.User.FindFirstValue("email") ?? httpContext.User.FindFirstValue(ClaimTypes.Email),
                memberships,
                activeMembership);

            httpContext.Items[HttpRequestContextAccessor.ItemKey] = context;

            if (TryReadRequestedTenantId(httpContext, out var requestedTenantId) &&
                memberships.All(membership => membership.TenantId != requestedTenantId))
            {
                logger.LogWarning(
                    "Forbidden tenant access attempt. Subject {SubjectId} requested tenant {TenantId}.",
                    context.SubjectId,
                    requestedTenantId);

                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    error = "tenant_forbidden",
                    detail = "The authenticated principal does not have access to the requested tenant."
                });
                return;
            }
        }

        await next(httpContext);
    }

    private static TenantMembership? ResolveActiveMembership(
        HttpContext httpContext,
        IReadOnlyCollection<TenantMembership> memberships)
    {
        if (memberships.Count == 0)
        {
            return null;
        }

        if (TryReadRequestedTenantId(httpContext, out var requestedTenantId))
        {
            return memberships.FirstOrDefault(membership => membership.TenantId == requestedTenantId);
        }

        if (memberships.Count == 1)
        {
            return memberships.First();
        }

        return null;
    }

    private static bool TryReadRequestedTenantId(HttpContext httpContext, out Guid tenantId)
    {
        var rawValue = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault()
                       ?? httpContext.User.FindFirstValue(HealthTechClaimTypes.TenantId);

        return Guid.TryParse(rawValue, out tenantId);
    }
}

public static class RequestContextServiceCollectionExtensions
{
    public static IServiceCollection AddHealthTechRequestContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.Configure<TenantAccessOptions>(configuration.GetSection(TenantAccessOptions.SectionName));
        services.AddSingleton<ITenantAccessProvider, BootstrapTenantAccessProvider>();
        services.AddScoped<IRequestContextAccessor, HttpRequestContextAccessor>();
        return services;
    }

    public static IApplicationBuilder UseHealthTechRequestContext(this IApplicationBuilder app)
        => app.UseMiddleware<RequestContextMiddleware>();
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddServiceDiscovery();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authSection = builder.Configuration.GetSection("Auth");
        options.Authority = authSection["Authority"];
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authSection["Issuer"],
            ValidateAudience = !string.IsNullOrWhiteSpace(authSection["Audience"]),
            ValidAudience = authSection["Audience"],
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services
    .AddReverseProxy()
    .AddServiceDiscoveryDestinationResolver()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var allowedTenants = builder.Configuration
    .GetSection("TenantAccess:Users")
    .GetChildren()
    .SelectMany(user =>
    {
        var subjectId = user["SubjectId"];
        var email = user["Email"];
        return user.GetSection("Memberships").GetChildren().Select(membership => new
        {
            SubjectId = subjectId,
            Email = email,
            TenantId = membership["TenantId"]
        });
    })
    .Where(entry => !string.IsNullOrWhiteSpace(entry.TenantId))
    .ToArray();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";

    if (!context.Request.Headers.ContainsKey("X-Correlation-Id"))
    {
        context.Request.Headers["X-Correlation-Id"] = Guid.NewGuid().ToString("N");
    }

    context.Response.Headers["X-Correlation-Id"] = context.Request.Headers["X-Correlation-Id"].ToString();
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/health") ||
        context.Request.Path.StartsWithSegments("/identity/api/whoami/public"))
    {
        await next();
        return;
    }

    if (context.User.Identity?.IsAuthenticated != true)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { error = "unauthorized" });
        return;
    }

    if (context.Request.Path.StartsWithSegments("/patients") || context.Request.Path.StartsWithSegments("/appointments"))
    {
        var subjectId = context.User.FindFirstValue("sub") ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = context.User.FindFirstValue("email") ?? context.User.FindFirstValue(ClaimTypes.Email);
        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "tenant_required",
                detail = "Set the X-Tenant-Id header when calling tenant-scoped APIs."
            });
            return;
        }

        var allowed = allowedTenants.Any(entry =>
            string.Equals(entry.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
            ((!string.IsNullOrWhiteSpace(subjectId) && string.Equals(entry.SubjectId, subjectId, StringComparison.OrdinalIgnoreCase)) ||
             (!string.IsNullOrWhiteSpace(email) && string.Equals(entry.Email, email, StringComparison.OrdinalIgnoreCase))));

        if (!allowed)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "tenant_forbidden" });
            return;
        }
    }

    await next();
});

app.MapReverseProxy();
app.Run();

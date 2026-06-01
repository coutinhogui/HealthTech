using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IO;
using HealthTech.BuildingBlocks.Abstractions;
using HealthTech.Gateway.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddServiceDiscovery();
builder.Services.Configure<BffAuthOptions>(builder.Configuration.GetSection(BffAuthOptions.SectionName));
builder.Services.Configure<TenantAccessOptions>(builder.Configuration.GetSection(TenantAccessOptions.SectionName));
builder.Services.AddSingleton<ITenantAccessProvider, GatewayTenantAccessProvider>();
builder.Services.AddHttpClient<IBffAuthService, BffAuthService>();
builder.Services.AddSingleton<IBffOnboardingService, PostgresBffOnboardingService>();
var dataProtectionBuilder = builder.Services.AddDataProtection()
    .SetApplicationName("HealthTech.Gateway");
var dataProtectionPath = builder.Configuration["Bff:DataProtectionPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    Directory.CreateDirectory(dataProtectionPath);
    dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));
}
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(BffCookieDefaults.Apply)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppShell", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length > 0)
        {
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var key = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? context.Connection.RemoteIpAddress?.ToString()
                  ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = 120,
            QueueLimit = 0,
            Window = TimeSpan.FromMinutes(1)
        });
    });
});
builder.Services
    .AddReverseProxy()
    .AddServiceDiscoveryDestinationResolver()
    .AddTransforms(transformBuilderContext =>
    {
transformBuilderContext.AddRequestTransform(async transformContext =>
        {
            // Always enforce internal headers server-side; never trust client-provided values.
            transformContext.ProxyRequest.Headers.Remove(InternalGatewayAuthenticationDefaults.SecretHeader);
            transformContext.ProxyRequest.Headers.Remove(InternalGatewayAuthenticationDefaults.SubjectHeader);
            transformContext.ProxyRequest.Headers.Remove(InternalGatewayAuthenticationDefaults.EmailHeader);
            transformContext.ProxyRequest.Headers.Remove("X-Tenant-Id");

            transformContext.ProxyRequest.Headers.Authorization = null;

            var internalSecret = builder.Configuration["InternalGateway:SharedSecret"];
            var subject = transformContext.HttpContext.User.FindFirstValue("sub")
                          ?? transformContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = transformContext.HttpContext.User.FindFirstValue("email")
                        ?? transformContext.HttpContext.User.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrWhiteSpace(internalSecret) && !string.IsNullOrWhiteSpace(subject))
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation(
                    InternalGatewayAuthenticationDefaults.SecretHeader,
                    internalSecret);
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation(
                    InternalGatewayAuthenticationDefaults.SubjectHeader,
                    subject);

                if (!string.IsNullOrWhiteSpace(email))
                {
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation(
                        InternalGatewayAuthenticationDefaults.EmailHeader,
                        email);
                }
            }
            else
            {
                var token = await transformContext.HttpContext.GetTokenAsync("access_token");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            var tenantId = transformContext.HttpContext.User.FindFirstValue(HealthTechClaimTypes.TenantId);
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Tenant-Id", tenantId);
            }
        });
    })
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

    if (!context.Request.Headers.ContainsKey("X-Correlation-Id"))
    {
        context.Request.Headers["X-Correlation-Id"] = Guid.NewGuid().ToString("N");
    }

    context.Response.Headers["X-Correlation-Id"] = context.Request.Headers["X-Correlation-Id"].ToString();
    await next();
});

app.UseCors("AppShell");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();

app.MapGet("/api/session", async (
    ClaimsPrincipal user,
    IBffOnboardingService onboardingService,
    CancellationToken cancellationToken) =>
{
    if (user.Identity?.IsAuthenticated != true)
    {
        return Results.Ok(new BffSessionResponse(false, null, null, null, []));
    }

    var memberships = ReadMemberships(user);
    var activeTenantId = user.FindFirstValue(HealthTechClaimTypes.TenantId);
    var activeTenant = memberships.FirstOrDefault(membership =>
        string.Equals(membership.TenantId.ToString("D"), activeTenantId, StringComparison.OrdinalIgnoreCase));

    var subject = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
    var email = user.FindFirstValue("email") ?? user.FindFirstValue(ClaimTypes.Email);
    var status = await onboardingService.GetStatusAsync(
        subject ?? string.Empty,
        email,
        activeTenant,
        memberships,
        cancellationToken);
    var decision = BffOnboardingDecision.Create(true, memberships.Count > 0, status.IsComplete);

    return Results.Ok(new BffSessionResponse(
        true,
        subject,
        email,
        activeTenant,
        memberships,
        decision.RequiresOnboarding));
}).AllowAnonymous();

app.MapPost("/api/session/tenant", async (
    BffSelectTenantRequest request,
    HttpContext httpContext,
    IBffOnboardingService onboardingService) =>
{
    var memberships = ReadMemberships(httpContext.User);
    var selectedTenant = memberships.FirstOrDefault(membership => membership.TenantId == request.TenantId);
    if (selectedTenant is null)
    {
        return Results.Json(new { error = "tenant_forbidden" }, statusCode: StatusCodes.Status403Forbidden);
    }

    var signIn = new BffSignInResult(
        true,
        null,
        httpContext.User.FindFirstValue("sub") ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
        httpContext.User.FindFirstValue("email") ?? httpContext.User.FindFirstValue(ClaimTypes.Email),
        await httpContext.GetTokenAsync("access_token"),
        await httpContext.GetTokenAsync("refresh_token"),
        memberships);

    var session = await SignInWithBffCookieAsync(
        httpContext,
        signIn,
        request.TenantId,
        signIn.Email ?? signIn.SubjectId ?? "user",
        onboardingService);

    return Results.Ok(session);
}).RequireAuthorization();

app.MapGet("/api/auth/providers", (IOptions<BffAuthOptions> authOptions)
    => Results.Ok(BffAuthCapabilities.Create(authOptions.Value))).AllowAnonymous();

app.MapPost("/api/auth/login", async (
    BffLoginRequest request,
    IBffAuthService authService,
    IBffOnboardingService onboardingService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var signIn = await authService.SignInWithPasswordAsync(request, cancellationToken);
    if (!signIn.Success)
    {
        return Results.Unauthorized();
    }

    var session = await SignInWithBffCookieAsync(httpContext, signIn, request.TenantId, request.Email, onboardingService);
    return Results.Ok(session);
}).AllowAnonymous();

app.MapPost("/api/auth/register", async (
    BffRegisterRequest request,
    IBffAuthService authService,
    IBffOnboardingService onboardingService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var signIn = await authService.RegisterWithPasswordAsync(request, cancellationToken);
    if (!signIn.Success)
    {
        return Results.BadRequest(new { error = signIn.Error ?? "registration_failed" });
    }

    var session = await SignInWithBffCookieAsync(httpContext, signIn, request.TenantId, request.Email, onboardingService);
    return Results.Ok(session);
}).AllowAnonymous();

app.MapPost("/api/onboarding", async (
    BffCompleteOnboardingRequest request,
    HttpContext httpContext,
    IBffOnboardingService onboardingService,
    CancellationToken cancellationToken) =>
{
    var errors = ValidateOnboardingRequest(request);
    if (errors.Count > 0)
    {
        return Results.BadRequest(new { error = "onboarding_invalid", fields = errors });
    }

    var subject = httpContext.User.FindFirstValue("sub") ??
                  httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrWhiteSpace(subject))
    {
        return Results.Unauthorized();
    }

    var email = httpContext.User.FindFirstValue("email") ??
                httpContext.User.FindFirstValue(ClaimTypes.Email);
    var completion = await onboardingService.CompleteAsync(subject, email, request, cancellationToken);
    var signIn = new BffSignInResult(
        true,
        null,
        subject,
        email,
        await httpContext.GetTokenAsync("access_token"),
        await httpContext.GetTokenAsync("refresh_token"),
        completion.Memberships);

    var session = await SignInWithBffCookieAsync(
        httpContext,
        signIn,
        completion.ActiveTenant.TenantId,
        email ?? subject,
        onboardingService);

    return Results.Ok(session);
}).RequireAuthorization();

app.MapPost("/api/auth/password/recovery", async (
    BffPasswordRecoveryRequest request,
    IBffAuthService authService,
    CancellationToken cancellationToken) =>
{
    var result = await authService.RequestPasswordRecoveryAsync(request.Email, request.RedirectTo, cancellationToken);
    if (!result.Success)
    {
        return Results.BadRequest(new { error = result.Error ?? "password_recovery_failed" });
    }

    return Results.Ok(new { sent = true });
}).AllowAnonymous();

app.MapGet("/api/auth/login/{provider}", (
    string provider,
    string? redirectTo,
    string? returnUrl,
    HttpContext httpContext,
    IOptions<BffAuthOptions> authOptions,
    IDataProtectionProvider dataProtectionProvider) =>
{
    var options = authOptions.Value;
    if (string.IsNullOrWhiteSpace(options.SupabaseUrl) || string.IsNullOrWhiteSpace(options.SupabaseAnonKey))
    {
        return Results.Problem(
            title: "Supabase OAuth is not configured.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    if (!options.OAuthProviders.Any(allowed => string.Equals(allowed, provider, StringComparison.OrdinalIgnoreCase)))
    {
        return Results.BadRequest(new { error = "oauth_provider_not_allowed" });
    }

    var state = BffOAuthFlow.CreateState();
    var codeVerifier = BffOAuthFlow.CreateCodeVerifier();
    var codeChallenge = BffOAuthFlow.CreateCodeChallenge(codeVerifier);
    var callbackUri = BffOAuthFlow.ResolveCallbackUri(
        httpContext.Request,
        options.PublicBaseUrl,
        redirectTo,
        builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? []);

    var correlation = new BffOAuthCorrelation(state, codeVerifier, BffOAuthFlow.NormalizeReturnUrl(returnUrl));
    var protector = dataProtectionProvider.CreateProtector("HealthTech.Bff.OAuth");
    var protectedCorrelation = protector.Protect(JsonSerializer.Serialize(correlation));
    httpContext.Response.Cookies.Append(
        BffOAuthFlow.CorrelationCookieName,
        protectedCorrelation,
        BffOAuthFlow.CreateCorrelationCookieOptions());

    var authorizationUrl = BffOAuthFlow.BuildAuthorizeUrl(options, provider, callbackUri, state, codeChallenge);
    return Results.Redirect(authorizationUrl);
}).AllowAnonymous();

app.MapGet("/api/auth/callback", async (
    string? code,
    string? state,
    string? error,
    string? responseMode,
    HttpContext httpContext,
    IBffAuthService authService,
    IBffOnboardingService onboardingService,
    IDataProtectionProvider dataProtectionProvider,
    CancellationToken cancellationToken) =>
{
    if (!string.IsNullOrWhiteSpace(error))
    {
        return Results.Problem(title: "OAuth provider returned an error.", detail: error, statusCode: StatusCodes.Status401Unauthorized);
    }

    if (string.IsNullOrWhiteSpace(code))
    {
        return Results.BadRequest(new { error = "oauth_code_required" });
    }

    if (!httpContext.Request.Cookies.TryGetValue(BffOAuthFlow.CorrelationCookieName, out var protectedCorrelation))
    {
        return Results.BadRequest(new { error = "oauth_correlation_missing" });
    }

    var protector = dataProtectionProvider.CreateProtector("HealthTech.Bff.OAuth");
    BffOAuthCorrelation? correlation;
    try
    {
        correlation = JsonSerializer.Deserialize<BffOAuthCorrelation>(protector.Unprotect(protectedCorrelation));
    }
    catch
    {
        return Results.BadRequest(new { error = "oauth_correlation_invalid" });
    }
    finally
    {
        httpContext.Response.Cookies.Delete(BffOAuthFlow.CorrelationCookieName, BffOAuthFlow.CreateCorrelationCookieOptions());
    }

    if (correlation is null ||
        (!string.IsNullOrWhiteSpace(state) &&
         !string.Equals(correlation.State, state, StringComparison.Ordinal)))
    {
        return Results.BadRequest(new { error = "oauth_state_mismatch" });
    }

    var signIn = await authService.ExchangeOAuthCodeAsync(code, correlation.CodeVerifier, cancellationToken);
    if (!signIn.Success)
    {
        return Results.Unauthorized();
    }

    var session = await SignInWithBffCookieAsync(
        httpContext,
        signIn,
        null,
        signIn.Email ?? "oauth-user",
        onboardingService);
    if (string.Equals(responseMode, "json", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Ok(session);
    }

    return Results.Redirect(correlation.ReturnUrl);
}).AllowAnonymous();

app.MapPost("/api/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.NoContent();
}).RequireAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/health") ||
        context.Request.Path.StartsWithSegments("/api/session") ||
        context.Request.Path.StartsWithSegments("/api/auth") ||
        context.Request.Path.StartsWithSegments("/api/identity/whoami/public"))
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

    if (context.Request.Path.StartsWithSegments("/api/patients") ||
        context.Request.Path.StartsWithSegments("/api/appointments") ||
        context.Request.Path.StartsWithSegments("/api/professionals") ||
        context.Request.Path.StartsWithSegments("/api/specialties") ||
        context.Request.Path.StartsWithSegments("/api/locations"))
    {
        var activeTenantId = context.User.FindFirstValue(HealthTechClaimTypes.TenantId);
        var requestedTenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(activeTenantId) &&
            !string.IsNullOrWhiteSpace(requestedTenantId) &&
            !string.Equals(activeTenantId, requestedTenantId, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "tenant_forbidden" });
            return;
        }

        var tenantId = activeTenantId ?? requestedTenantId;

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

        var tenantAccessProvider = context.RequestServices.GetRequiredService<ITenantAccessProvider>();
        var allowedMemberships = await tenantAccessProvider.GetMembershipsAsync(context.User, context.RequestAborted);
        var allowed = allowedMemberships.Any(membership =>
            string.Equals(membership.TenantId.ToString("D"), tenantId, StringComparison.OrdinalIgnoreCase));

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

static IReadOnlyCollection<BffTenantResponse> ReadMemberships(ClaimsPrincipal user)
    => user.FindAll("tenant_membership")
        .Select(claim => claim.Value.Split('|', 3))
        .Where(parts => parts.Length == 3 && Guid.TryParse(parts[0], out _))
        .Select(parts => new BffTenantResponse(Guid.Parse(parts[0]), parts[1], parts[2]))
        .ToArray();

static async Task<BffSessionResponse> SignInWithBffCookieAsync(
    HttpContext httpContext,
    BffSignInResult signIn,
    Guid? requestedTenantId,
    string fallbackEmail,
    IBffOnboardingService onboardingService)
{
    var activeTenant = ResolveActiveTenant(signIn.Memberships, requestedTenantId);
    var subject = signIn.SubjectId ?? signIn.Email ?? fallbackEmail;
    var email = signIn.Email ?? fallbackEmail;
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, subject),
        new("sub", subject),
        new(ClaimTypes.Email, email),
        new("email", email)
    };

    foreach (var membership in signIn.Memberships)
    {
        claims.Add(new("tenant_membership", $"{membership.TenantId:D}|{membership.TenantName}|{membership.Role}"));
    }

    if (activeTenant is not null)
    {
        claims.Add(new(HealthTechClaimTypes.TenantId, activeTenant.TenantId.ToString("D")));
        claims.Add(new(HealthTechClaimTypes.TenantName, activeTenant.TenantName));
        claims.Add(new(HealthTechClaimTypes.TenantRole, activeTenant.Role));
    }

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    var properties = new AuthenticationProperties
    {
        IsPersistent = true,
        AllowRefresh = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
    };

    if (!string.IsNullOrWhiteSpace(signIn.AccessToken))
    {
        properties.StoreTokens([
            new AuthenticationToken { Name = "access_token", Value = signIn.AccessToken },
            new AuthenticationToken { Name = "refresh_token", Value = signIn.RefreshToken ?? string.Empty }
        ]);
    }

    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
    var status = await onboardingService.GetStatusAsync(subject, email, activeTenant, signIn.Memberships, httpContext.RequestAborted);
    var decision = BffOnboardingDecision.Create(true, signIn.Memberships.Count > 0, status.IsComplete);
    return new BffSessionResponse(true, signIn.SubjectId, signIn.Email, activeTenant, signIn.Memberships, decision.RequiresOnboarding);
}

static Dictionary<string, string[]> ValidateOnboardingRequest(BffCompleteOnboardingRequest request)
{
    var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    AddRequired(errors, nameof(request.ClinicName), request.ClinicName, 160);
    AddRequired(errors, nameof(request.ResponsibleName), request.ResponsibleName, 160);
    AddRequired(errors, nameof(request.Phone), request.Phone, 32);
    AddRequired(errors, nameof(request.SpecialtyName), request.SpecialtyName, 120);
    AddRequired(errors, nameof(request.LocationName), request.LocationName, 120);
    AddRequired(errors, nameof(request.Timezone), request.Timezone, 64);
    return errors;
}

static void AddRequired(Dictionary<string, string[]> errors, string field, string value, int maxLength)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        errors[field] = ["Campo obrigatorio."];
        return;
    }

    if (value.Trim().Length > maxLength)
    {
        errors[field] = [$"Use no maximo {maxLength} caracteres."];
    }
}

static BffTenantResponse? ResolveActiveTenant(IReadOnlyCollection<BffTenantResponse> memberships, Guid? requestedTenantId)
{
    if (memberships.Count == 0)
    {
        return null;
    }

    if (requestedTenantId is not null)
    {
        return memberships.FirstOrDefault(membership => membership.TenantId == requestedTenantId.Value);
    }

    return memberships.Count == 1 ? memberships.First() : null;
}

public partial class Program;

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
builder.Services.Configure<SystemAdminOptions>(builder.Configuration.GetSection(SystemAdminOptions.SectionName));
builder.Services.AddSingleton<ITenantAccessProvider, GatewayTenantAccessProvider>();
builder.Services.AddHttpClient<IBffAuthService, BffAuthService>();
builder.Services.AddSingleton<IBffOnboardingService, PostgresBffOnboardingService>();
builder.Services.AddSingleton<IBffClinicAccessService, PostgresBffClinicAccessService>();
builder.Services.AddSingleton<IBffSaaSAdminService, PostgresBffSaaSAdminService>();
builder.Services.AddSingleton<IBffDiscoveryService, PostgresBffDiscoveryService>();
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
    options.AddPolicy("Front", policy =>
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

app.UseCors("Front");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();

app.MapGet("/api/session", async (
    HttpContext httpContext,
    ITenantAccessProvider tenantAccessProvider,
    IBffOnboardingService onboardingService,
    IBffSaaSAdminService saasAdminService,
    CancellationToken cancellationToken) =>
{
    var user = httpContext.User;
    if (user.Identity?.IsAuthenticated != true)
    {
        return Results.Ok(new BffSessionResponse(false, null, null, null, []));
    }

    var subject = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
    var email = user.FindFirstValue("email") ?? user.FindFirstValue(ClaimTypes.Email);
    var memberships = (await tenantAccessProvider.GetMembershipsAsync(user, cancellationToken))
        .Select(ToBffTenantResponse)
        .ToArray();
    var activeTenantId = user.FindFirstValue(HealthTechClaimTypes.TenantId);
    var requestedTenantId = Guid.TryParse(activeTenantId, out var parsedTenantId)
        ? parsedTenantId
        : (Guid?)null;
    var accessArea = requestedTenantId is null &&
                     string.Equals(user.FindFirstValue(HealthTechClaimTypes.GlobalRole), ClinicRoles.SystemAdmin, StringComparison.OrdinalIgnoreCase)
        ? BffAccessAreas.Environment
        : BffAccessAreas.Clinic;
    var signIn = new BffSignInResult(
        true,
        null,
        subject,
        email,
        await httpContext.GetTokenAsync("access_token"),
        await httpContext.GetTokenAsync("refresh_token"),
        memberships);

    return Results.Ok(await SignInWithBffCookieAsync(
        httpContext,
        signIn,
        requestedTenantId,
        email ?? subject ?? "user",
        onboardingService,
        saasAdminService,
        accessArea));
}).AllowAnonymous();

app.MapPost("/api/session/tenant", async (
    BffSelectTenantRequest request,
    HttpContext httpContext,
    ITenantAccessProvider tenantAccessProvider,
    IBffOnboardingService onboardingService,
    IBffSaaSAdminService saasAdminService) =>
{
    var memberships = (await tenantAccessProvider.GetMembershipsAsync(httpContext.User, httpContext.RequestAborted))
        .Select(ToBffTenantResponse)
        .ToArray();
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
        onboardingService,
        saasAdminService);

    return Results.Ok(session);
}).RequireAuthorization();

app.MapGet("/api/auth/providers", (IOptions<BffAuthOptions> authOptions)
    => Results.Ok(BffAuthCapabilities.Create(authOptions.Value))).AllowAnonymous();

app.MapGet("/api/discovery/clinics", async (
    IBffDiscoveryService discoveryService,
    CancellationToken cancellationToken) =>
{
    return Results.Ok(await discoveryService.ListClinicsAsync(cancellationToken));
}).AllowAnonymous();

app.MapGet("/api/discovery/search", async (
    string? mode,
    string? query,
    string? clinic,
    string? specialty,
    string? region,
    int? take,
    double? latitude,
    double? longitude,
    IBffDiscoveryService discoveryService,
    CancellationToken cancellationToken) =>
{
    var request = new BffDiscoverySearchRequest(
        NormalizeDiscoveryMode(mode),
        query,
        clinic,
        specialty,
        region,
        take ?? 20,
        latitude,
        longitude);
    var errors = ValidateDiscoverySearchRequest(request);
    if (errors.Count > 0)
    {
        return Results.BadRequest(new { error = "discovery_search_invalid", fields = errors });
    }

    var normalized = request with
    {
        Query = NormalizeOptionalQuery(request.Query),
        Clinic = NormalizeOptionalQuery(request.Clinic),
        Specialty = NormalizeOptionalQuery(request.Specialty),
        Region = NormalizeOptionalQuery(request.Region),
        Take = Math.Clamp(request.Take, 1, 50),
        Latitude = request.Latitude,
        Longitude = request.Longitude
    };
    return Results.Ok(await discoveryService.SearchAsync(normalized, cancellationToken));
}).AllowAnonymous();

app.MapGet("/api/discovery/specialties", async (
    Guid tenantId,
    IBffDiscoveryService discoveryService,
    CancellationToken cancellationToken) =>
{
    if (tenantId == Guid.Empty)
    {
        return Results.BadRequest(new { error = "tenant_required" });
    }

    return Results.Ok(await discoveryService.ListSpecialtiesAsync(tenantId, cancellationToken));
}).AllowAnonymous();

app.MapGet("/api/discovery/professionals", async (
    Guid tenantId,
    Guid? specialtyId,
    IBffDiscoveryService discoveryService,
    CancellationToken cancellationToken) =>
{
    if (tenantId == Guid.Empty)
    {
        return Results.BadRequest(new { error = "tenant_required" });
    }

    return Results.Ok(await discoveryService.ListProfessionalsAsync(tenantId, specialtyId, cancellationToken));
}).AllowAnonymous();

app.MapGet("/api/discovery/slots", async (
    Guid tenantId,
    Guid professionalId,
    Guid? locationId,
    DateTimeOffset? fromUtc,
    DateTimeOffset? toUtc,
    int? slotMinutes,
    IBffDiscoveryService discoveryService,
    CancellationToken cancellationToken) =>
{
    var from = fromUtc ?? DateTimeOffset.UtcNow.AddHours(1);
    var to = toUtc ?? from.AddDays(7);
    var duration = slotMinutes ?? 30;
    var errors = ValidateDiscoverySlotsRequest(tenantId, professionalId, from, to, duration);
    if (errors.Count > 0)
    {
        return Results.BadRequest(new { error = "discovery_slots_invalid", fields = errors });
    }

    return Results.Ok(await discoveryService.ListSlotsAsync(
        tenantId,
        professionalId,
        locationId,
        from,
        to,
        duration,
        cancellationToken));
}).AllowAnonymous();

app.MapPost("/api/discovery/appointments", async (
    BffDiscoveryBookingRequest request,
    IBffDiscoveryService discoveryService,
    CancellationToken cancellationToken) =>
{
    var requestWithEnd = request.EndsAtUtc == default
        ? request with { EndsAtUtc = request.StartsAtUtc.AddMinutes(30) }
        : request;
    var errors = ValidateDiscoveryBookingRequest(requestWithEnd);
    if (errors.Count > 0)
    {
        return Results.BadRequest(new { error = "discovery_booking_invalid", fields = errors });
    }

    var result = await discoveryService.BookAsync(requestWithEnd, cancellationToken);
    if (result.Success)
    {
        return Results.Created($"/api/discovery/appointments/{result.AppointmentId:D}", new { id = result.AppointmentId });
    }

    return result.Code switch
    {
        "appointment_conflict" => Results.Conflict(new { error = result.Code, message = result.Message }),
        "clinic_not_found" or "professional_not_found" or "location_not_found" => Results.NotFound(new { error = result.Code, message = result.Message }),
        _ => Results.BadRequest(new { error = result.Code ?? "discovery_booking_failed", message = result.Message })
    };
}).AllowAnonymous();

app.MapPost("/api/auth/login", async (
    BffLoginRequest request,
    IBffAuthService authService,
    IBffOnboardingService onboardingService,
    IBffSaaSAdminService saasAdminService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var signIn = await authService.SignInWithPasswordAsync(request, cancellationToken);
    if (!signIn.Success)
    {
        return Results.Unauthorized();
    }

    var accessArea = NormalizeAccessArea(request.AccessArea);
    var accessError = await ValidateAccessAreaAsync(accessArea, signIn, request.Email, saasAdminService, cancellationToken);
    if (accessError is not null)
    {
        return Results.Json(new { error = accessError }, statusCode: StatusCodes.Status403Forbidden);
    }

    var session = await SignInWithBffCookieAsync(httpContext, signIn, request.TenantId, request.Email, onboardingService, saasAdminService, accessArea);
    return Results.Ok(session);
}).AllowAnonymous();

app.MapPost("/api/auth/register", async (
    BffRegisterRequest request,
    IBffAuthService authService,
    IBffOnboardingService onboardingService,
    IBffSaaSAdminService saasAdminService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var signIn = await authService.RegisterWithPasswordAsync(request, cancellationToken);
    if (!signIn.Success)
    {
        return Results.BadRequest(new { error = signIn.Error ?? "registration_failed" });
    }

    var accessArea = NormalizeAccessArea(request.AccessArea);
    var accessError = await ValidateAccessAreaAsync(accessArea, signIn, request.Email, saasAdminService, cancellationToken);
    if (accessError is not null)
    {
        return Results.Json(new { error = accessError }, statusCode: StatusCodes.Status403Forbidden);
    }

    var session = await SignInWithBffCookieAsync(httpContext, signIn, request.TenantId, request.Email, onboardingService, saasAdminService, accessArea);
    return Results.Ok(session);
}).AllowAnonymous();

app.MapPost("/api/onboarding", async (
    BffCompleteOnboardingRequest request,
    HttpContext httpContext,
    IBffOnboardingService onboardingService,
    IBffSaaSAdminService saasAdminService,
    IOptions<BffAuthOptions> authOptions,
    CancellationToken cancellationToken) =>
{
    if (!authOptions.Value.EnablePublicOnboarding)
    {
        return Results.Json(new { error = "onboarding_disabled" }, statusCode: StatusCodes.Status403Forbidden);
    }

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
        onboardingService,
        saasAdminService);

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
    string? accessArea,
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

    var normalizedAccessArea = NormalizeAccessArea(accessArea);
    var normalizedReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
        ? GetDefaultReturnUrlForAccessArea(normalizedAccessArea)
        : BffOAuthFlow.NormalizeReturnUrl(returnUrl);
    var correlation = new BffOAuthCorrelation(state, codeVerifier, normalizedReturnUrl, normalizedAccessArea);
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
    IBffSaaSAdminService saasAdminService,
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

    var accessError = await ValidateAccessAreaAsync(
        NormalizeAccessArea(correlation.AccessArea),
        signIn,
        signIn.Email ?? "oauth-user",
        saasAdminService,
        cancellationToken);
    if (accessError is not null)
    {
        return Results.Json(new { error = accessError }, statusCode: StatusCodes.Status403Forbidden);
    }

    var session = await SignInWithBffCookieAsync(
        httpContext,
        signIn,
        null,
        signIn.Email ?? "oauth-user",
        onboardingService,
        saasAdminService,
        NormalizeAccessArea(correlation.AccessArea));
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

app.MapGet("/api/admin/clinics", async (
    ClaimsPrincipal user,
    IBffSaaSAdminService saasAdminService,
    CancellationToken cancellationToken) =>
{
    if (!await RequireSystemAdminAsync(user, saasAdminService, cancellationToken))
    {
        return Results.Json(new { error = "system_admin_required" }, statusCode: StatusCodes.Status403Forbidden);
    }

    var subject = GetRequiredSubject(user);
    var email = user.FindFirstValue("email") ?? user.FindFirstValue(ClaimTypes.Email);
    return Results.Ok(await saasAdminService.ListClinicsAsync(subject, email, cancellationToken));
}).RequireAuthorization();

app.MapPost("/api/admin/clinics", async (
    BffCreateClinicRequest request,
    ClaimsPrincipal user,
    IBffSaaSAdminService saasAdminService,
    CancellationToken cancellationToken) =>
{
    if (!await RequireSystemAdminAsync(user, saasAdminService, cancellationToken))
    {
        return Results.Json(new { error = "system_admin_required" }, statusCode: StatusCodes.Status403Forbidden);
    }

    var subject = GetRequiredSubject(user);
    var email = user.FindFirstValue("email") ?? user.FindFirstValue(ClaimTypes.Email);
    var clinic = await saasAdminService.CreateClinicAsync(subject, email, request, cancellationToken);
    return Results.Ok(clinic);
}).RequireAuthorization();

app.MapPut("/api/admin/clinics/{tenantId:guid}/status", async (
    Guid tenantId,
    BffUpdateClinicStatusRequest request,
    ClaimsPrincipal user,
    IBffSaaSAdminService saasAdminService,
    CancellationToken cancellationToken) =>
{
    if (!await RequireSystemAdminAsync(user, saasAdminService, cancellationToken))
    {
        return Results.Json(new { error = "system_admin_required" }, statusCode: StatusCodes.Status403Forbidden);
    }

    var subject = GetRequiredSubject(user);
    var email = user.FindFirstValue("email") ?? user.FindFirstValue(ClaimTypes.Email);
    var clinic = await saasAdminService.UpdateClinicStatusAsync(subject, email, tenantId, request, cancellationToken);
    return clinic is null ? Results.NotFound(new { error = "clinic_not_found" }) : Results.Ok(clinic);
}).RequireAuthorization();

app.MapPost("/api/admin/clinics/{tenantId:guid}/admins", async (
    Guid tenantId,
    BffCreateClinicAdminRequest request,
    ClaimsPrincipal user,
    IBffSaaSAdminService saasAdminService,
    CancellationToken cancellationToken) =>
{
    if (!await RequireSystemAdminAsync(user, saasAdminService, cancellationToken))
    {
        return Results.Json(new { error = "system_admin_required" }, statusCode: StatusCodes.Status403Forbidden);
    }

    var subject = GetRequiredSubject(user);
    var email = user.FindFirstValue("email") ?? user.FindFirstValue(ClaimTypes.Email);
    var updated = await saasAdminService.AddClinicAdminAsync(subject, email, tenantId, request, cancellationToken);
    return updated ? Results.NoContent() : Results.NotFound(new { error = "clinic_not_found" });
}).RequireAuthorization();

app.MapGet("/api/admin/system-users", async (
    ClaimsPrincipal user,
    IBffSaaSAdminService saasAdminService,
    CancellationToken cancellationToken) =>
{
    if (!await RequireSystemAdminAsync(user, saasAdminService, cancellationToken))
    {
        return Results.Json(new { error = "system_admin_required" }, statusCode: StatusCodes.Status403Forbidden);
    }

    var subject = GetRequiredSubject(user);
    var email = user.FindFirstValue("email") ?? user.FindFirstValue(ClaimTypes.Email);
    return Results.Ok(await saasAdminService.ListSystemAdminsAsync(subject, email, cancellationToken));
}).RequireAuthorization();

app.MapPut("/api/admin/system-users/{subjectId}", async (
    string subjectId,
    BffUpdateSystemAdminRequest request,
    ClaimsPrincipal user,
    IBffSaaSAdminService saasAdminService,
    CancellationToken cancellationToken) =>
{
    if (!await RequireSystemAdminAsync(user, saasAdminService, cancellationToken))
    {
        return Results.Json(new { error = "system_admin_required" }, statusCode: StatusCodes.Status403Forbidden);
    }

    var subject = GetRequiredSubject(user);
    var email = user.FindFirstValue("email") ?? user.FindFirstValue(ClaimTypes.Email);
    var updated = await saasAdminService.UpdateSystemAdminAsync(subject, email, subjectId, request, cancellationToken);
    return updated ? Results.NoContent() : Results.BadRequest(new { error = "system_admin_update_failed" });
}).RequireAuthorization();

app.MapGet("/api/access/users", async (
    ClaimsPrincipal user,
    IBffClinicAccessService accessService,
    CancellationToken cancellationToken) =>
{
    var activeTenant = ReadActiveTenant(user);
    if (activeTenant is null)
    {
        return Results.BadRequest(new { error = "tenant_required" });
    }

    if (!ClinicAuthorization.RoleHasPermission(activeTenant.Role, ClinicPermission.ManageAccess))
    {
        return Results.Json(new { error = "role_forbidden" }, statusCode: StatusCodes.Status403Forbidden);
    }

    var users = await accessService.ListUsersAsync(activeTenant.TenantId, cancellationToken);
    return Results.Ok(users);
}).RequireAuthorization();

app.MapPut("/api/access/users/{subjectId}", async (
    string subjectId,
    BffUpdateAccessUserRequest request,
    ClaimsPrincipal user,
    IBffClinicAccessService accessService,
    CancellationToken cancellationToken) =>
{
    var activeTenant = ReadActiveTenant(user);
    if (activeTenant is null)
    {
        return Results.BadRequest(new { error = "tenant_required" });
    }

    if (!ClinicAuthorization.RoleHasPermission(activeTenant.Role, ClinicPermission.ManageAccess))
    {
        return Results.Json(new { error = "role_forbidden" }, statusCode: StatusCodes.Status403Forbidden);
    }

    var updated = await accessService.UpdateUserAsync(activeTenant.TenantId, subjectId, request, cancellationToken);
    return updated ? Results.NoContent() : Results.BadRequest(new { error = "access_update_failed" });
}).RequireAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/health") ||
        context.Request.Path.StartsWithSegments("/api/session") ||
        context.Request.Path.StartsWithSegments("/api/auth") ||
        context.Request.Path.StartsWithSegments("/api/discovery") ||
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
        context.Request.Path.StartsWithSegments("/api/locations") ||
        context.Request.Path.StartsWithSegments("/api/access"))
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
            var error = string.IsNullOrWhiteSpace(activeTenantId) && !string.IsNullOrWhiteSpace(requestedTenantId)
                ? "tenant_forbidden"
                : "clinic_inactive_or_membership_revoked";
            await context.Response.WriteAsJsonAsync(new { error });
            return;
        }
    }

    await next();
});

app.MapReverseProxy();
app.Run();

static IReadOnlyCollection<BffTenantResponse> ReadMemberships(ClaimsPrincipal user)
    => user.FindAll("tenant_membership")
        .Select(claim => claim.Value.Split('|', 4))
        .Where(parts => parts.Length >= 3 && Guid.TryParse(parts[0], out _))
        .Select(parts =>
        {
            var role = parts[2];
            var professionalId = parts.Length == 4 && Guid.TryParse(parts[3], out var parsedProfessionalId)
                ? parsedProfessionalId
                : (Guid?)null;
            return new BffTenantResponse(
                Guid.Parse(parts[0]),
                parts[1],
                role,
                professionalId,
                ClinicAuthorization.PermissionNamesForRole(role));
        })
        .ToArray();

static BffTenantResponse? ReadActiveTenant(ClaimsPrincipal user)
{
    var memberships = ReadMemberships(user);
    var activeTenantId = user.FindFirstValue(HealthTechClaimTypes.TenantId);
    return memberships.FirstOrDefault(membership =>
        string.Equals(membership.TenantId.ToString("D"), activeTenantId, StringComparison.OrdinalIgnoreCase));
}

static BffTenantResponse ToBffTenantResponse(TenantMembership membership)
    => new(
        membership.TenantId,
        membership.TenantName,
        membership.Role,
        membership.ProfessionalId,
        ClinicAuthorization.PermissionNamesForRole(membership.Role));

static string GetRequiredSubject(ClaimsPrincipal user)
    => user.FindFirstValue("sub")
       ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
       ?? throw new InvalidOperationException("Authenticated principal is missing subject.");

static async Task<bool> IsSystemAdminSessionAsync(
    ClaimsPrincipal user,
    IBffSaaSAdminService saasAdminService,
    CancellationToken cancellationToken)
{
    if (string.Equals(
            user.FindFirstValue(HealthTechClaimTypes.GlobalRole),
            ClinicRoles.SystemAdmin,
            StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    var subject = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
    var email = user.FindFirstValue("email") ?? user.FindFirstValue(ClaimTypes.Email);
    return await saasAdminService.IsSystemAdminAsync(subject, email, cancellationToken);
}

static Task<bool> RequireSystemAdminAsync(
    ClaimsPrincipal user,
    IBffSaaSAdminService saasAdminService,
    CancellationToken cancellationToken)
    => IsSystemAdminSessionAsync(user, saasAdminService, cancellationToken);

static async Task<BffSessionResponse> SignInWithBffCookieAsync(
    HttpContext httpContext,
    BffSignInResult signIn,
    Guid? requestedTenantId,
    string fallbackEmail,
    IBffOnboardingService onboardingService,
    IBffSaaSAdminService saasAdminService,
    string? accessArea = null)
{
    var normalizedAccessArea = NormalizeAccessArea(accessArea);
    var activeTenant = string.Equals(normalizedAccessArea, BffAccessAreas.Environment, StringComparison.Ordinal)
        ? null
        : ResolveActiveTenant(signIn.Memberships, requestedTenantId);
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
        claims.Add(new("tenant_membership", $"{membership.TenantId:D}|{membership.TenantName}|{membership.Role}|{membership.ProfessionalId?.ToString("D") ?? string.Empty}"));
    }

    if (activeTenant is not null)
    {
        claims.Add(new(HealthTechClaimTypes.TenantId, activeTenant.TenantId.ToString("D")));
        claims.Add(new(HealthTechClaimTypes.TenantName, activeTenant.TenantName));
        claims.Add(new(HealthTechClaimTypes.TenantRole, activeTenant.Role));
    }

    var isSystemAdmin = await saasAdminService.IsSystemAdminAsync(subject, email, httpContext.RequestAborted);
    if (isSystemAdmin)
    {
        claims.Add(new(HealthTechClaimTypes.GlobalRole, ClinicRoles.SystemAdmin));
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
    var decision = isSystemAdmin
        ? new BffOnboardingDecision(false)
        : BffOnboardingDecision.Create(true, signIn.Memberships.Count > 0, status.IsComplete);
    return new BffSessionResponse(
        true,
        signIn.SubjectId,
        signIn.Email,
        activeTenant,
        signIn.Memberships,
        decision.RequiresOnboarding,
        isSystemAdmin,
        isSystemAdmin ? ClinicAuthorization.GlobalPermissionNamesForRole(ClinicRoles.SystemAdmin) : []);
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

static Dictionary<string, string[]> ValidateDiscoverySearchRequest(BffDiscoverySearchRequest request)
{
    var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    if (request.Mode is not ("professional" or "clinic"))
    {
        errors[nameof(request.Mode)] = ["Modo de busca invalido."];
    }

    AddOptionalMax(errors, nameof(request.Query), request.Query, 120);
    AddOptionalMax(errors, nameof(request.Clinic), request.Clinic, 120);
    AddOptionalMax(errors, nameof(request.Specialty), request.Specialty, 120);
    AddOptionalMax(errors, nameof(request.Region), request.Region, 120);
    if (request.Latitude is < -90 or > 90)
    {
        errors[nameof(request.Latitude)] = ["Latitude invalida."];
    }

    if (request.Longitude is < -180 or > 180)
    {
        errors[nameof(request.Longitude)] = ["Longitude invalida."];
    }

    if (request.Latitude.HasValue != request.Longitude.HasValue)
    {
        errors["coordinates"] = ["Informe latitude e longitude juntas."];
    }

    if (request.Take is < 1 or > 50)
    {
        errors[nameof(request.Take)] = ["Use um limite entre 1 e 50 resultados."];
    }

    return errors;
}

static string NormalizeDiscoveryMode(string? mode)
    => string.Equals(mode, "clinic", StringComparison.OrdinalIgnoreCase) ? "clinic" : "professional";

static string? NormalizeOptionalQuery(string? value)
    => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

static Dictionary<string, string[]> ValidateDiscoverySlotsRequest(
    Guid tenantId,
    Guid professionalId,
    DateTimeOffset fromUtc,
    DateTimeOffset toUtc,
    int slotMinutes)
{
    var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    if (tenantId == Guid.Empty)
    {
        errors["tenantId"] = ["Clinica obrigatoria."];
    }

    if (professionalId == Guid.Empty)
    {
        errors["professionalId"] = ["Profissional obrigatorio."];
    }

    if (toUtc <= fromUtc)
    {
        errors["toUtc"] = ["Periodo final deve ser maior que o inicial."];
    }

    if (slotMinutes is < 15 or > 120)
    {
        errors["slotMinutes"] = ["Duracao deve ficar entre 15 e 120 minutos."];
    }

    return errors;
}

static Dictionary<string, string[]> ValidateDiscoveryBookingRequest(BffDiscoveryBookingRequest request)
{
    var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    if (request.TenantId == Guid.Empty)
    {
        errors[nameof(request.TenantId)] = ["Clinica obrigatoria."];
    }

    if (request.ProfessionalId == Guid.Empty)
    {
        errors[nameof(request.ProfessionalId)] = ["Profissional obrigatorio."];
    }

    if (request.StartsAtUtc <= DateTimeOffset.UtcNow.AddMinutes(-5))
    {
        errors[nameof(request.StartsAtUtc)] = ["Escolha um horario futuro."];
    }

    if (request.EndsAtUtc <= request.StartsAtUtc)
    {
        errors[nameof(request.EndsAtUtc)] = ["Horario final invalido."];
    }

    AddRequired(errors, nameof(request.PatientName), request.PatientName, 160);
    AddRequired(errors, nameof(request.PatientDocument), request.PatientDocument, 32);
    if (request.PatientBirthDate == default || request.PatientBirthDate > DateOnly.FromDateTime(DateTime.UtcNow))
    {
        errors[nameof(request.PatientBirthDate)] = ["Data de nascimento invalida."];
    }

    if (string.IsNullOrWhiteSpace(request.PatientEmail) && string.IsNullOrWhiteSpace(request.PatientPhone))
    {
        errors[nameof(request.PatientEmail)] = ["Informe e-mail ou telefone para contato."];
    }

    if (!string.IsNullOrWhiteSpace(request.PatientEmail) && request.PatientEmail.Trim().Length > 180)
    {
        errors[nameof(request.PatientEmail)] = ["Use no maximo 180 caracteres."];
    }

    if (!string.IsNullOrWhiteSpace(request.PatientPhone) && request.PatientPhone.Trim().Length > 32)
    {
        errors[nameof(request.PatientPhone)] = ["Use no maximo 32 caracteres."];
    }

    if (!string.IsNullOrWhiteSpace(request.Notes) && request.Notes.Trim().Length > 500)
    {
        errors[nameof(request.Notes)] = ["Use no maximo 500 caracteres."];
    }

    return errors;
}

static void AddOptionalMax(Dictionary<string, string[]> errors, string field, string? value, int maxLength)
{
    if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > maxLength)
    {
        errors[field] = [$"Use no maximo {maxLength} caracteres."];
    }
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

static async Task<string?> ValidateAccessAreaAsync(
    string accessArea,
    BffSignInResult signIn,
    string fallbackEmail,
    IBffSaaSAdminService saasAdminService,
    CancellationToken cancellationToken)
{
    var subject = signIn.SubjectId ?? signIn.Email ?? fallbackEmail;
    var email = signIn.Email ?? fallbackEmail;
    var isSystemAdmin = await saasAdminService.IsSystemAdminAsync(subject, email, cancellationToken);

    if (string.Equals(accessArea, BffAccessAreas.Environment, StringComparison.Ordinal))
    {
        return isSystemAdmin ? null : "system_admin_required";
    }

    return signIn.Memberships.Count > 0 ? null : "clinic_membership_required";
}

static string NormalizeAccessArea(string? accessArea)
{
    if (string.Equals(accessArea, BffAccessAreas.Environment, StringComparison.OrdinalIgnoreCase))
    {
        return BffAccessAreas.Environment;
    }

    return BffAccessAreas.Clinic;
}

static string GetDefaultReturnUrlForAccessArea(string accessArea)
    => string.Equals(accessArea, BffAccessAreas.Environment, StringComparison.Ordinal)
        ? "/admin"
        : "/";

static class BffAccessAreas
{
    public const string Clinic = "clinic";
    public const string Environment = "environment";
}

public partial class Program;

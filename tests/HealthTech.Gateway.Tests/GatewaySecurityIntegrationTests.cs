using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HealthTech.Gateway.Tests;

public sealed class GatewaySecurityIntegrationTests
{
    private static readonly Guid TenantA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantB = Guid.Parse("99999999-9999-9999-9999-999999999999");

    public static TheoryData<string> TenantScopedRoutes =>
    [
        "/api/patients",
        "/api/appointments",
        "/api/professionals",
        "/api/locations",
        "/api/specialties"
    ];

    [Theory]
    [MemberData(nameof(TenantScopedRoutes))]
    public async Task Anonymous_user_receives_401_for_tenant_scoped_routes(string route)
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(backend.BaseAddress, withoutActiveTenant: false, allowTenantB: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(TenantScopedRoutes))]
    public async Task Authenticated_user_without_active_tenant_receives_tenant_required(string route)
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(backend.BaseAddress, withoutActiveTenant: true, allowTenantB: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var cookie = await LoginAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Get, route);
        request.Headers.TryAddWithoutValidation("Cookie", cookie);

        var response = await client.SendAsync(request);
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("tenant_required", payload.Error);
    }

    [Theory]
    [MemberData(nameof(TenantScopedRoutes))]
    public async Task Forged_tenant_header_without_membership_receives_tenant_forbidden(string route)
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(backend.BaseAddress, withoutActiveTenant: true, allowTenantB: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var cookie = await LoginAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Get, route);
        request.Headers.TryAddWithoutValidation("Cookie", cookie);
        request.Headers.TryAddWithoutValidation("X-Tenant-Id", TenantB.ToString("D"));

        var response = await client.SendAsync(request);
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("tenant_forbidden", payload.Error);
    }

    [Theory]
    [MemberData(nameof(TenantScopedRoutes))]
    public async Task Forged_tenant_header_with_active_cookie_receives_tenant_forbidden(string route)
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(backend.BaseAddress, withoutActiveTenant: false, allowTenantB: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var cookie = await LoginAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Get, route);
        request.Headers.TryAddWithoutValidation("Cookie", cookie);
        request.Headers.TryAddWithoutValidation("X-Tenant-Id", TenantB.ToString("D"));
        request.Headers.TryAddWithoutValidation("X-Internal-Gateway-Secret", "forged-secret");
        request.Headers.TryAddWithoutValidation("X-Internal-Subject", "forged-subject");
        request.Headers.TryAddWithoutValidation("X-Internal-Email", "forged@attacker.local");

        var response = await client.SendAsync(request);
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("tenant_forbidden", payload.Error);
    }

    [Theory]
    [MemberData(nameof(TenantScopedRoutes))]
    public async Task Gateway_injects_internal_headers_server_side(string route)
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(backend.BaseAddress, withoutActiveTenant: false, allowTenantB: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var cookie = await LoginAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Get, route);
        request.Headers.TryAddWithoutValidation("Cookie", cookie);
        request.Headers.TryAddWithoutValidation("X-Internal-Gateway-Secret", "forged-secret");
        request.Headers.TryAddWithoutValidation("X-Internal-Subject", "forged-subject");
        request.Headers.TryAddWithoutValidation("X-Internal-Email", "forged@attacker.local");

        var response = await client.SendAsync(request);
        var forwarded = await response.Content.ReadFromJsonAsync<ForwardedRequestEcho>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(forwarded);

        Assert.True(forwarded.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader));
        Assert.Equal(TenantA.ToString("D"), tenantHeader);

        Assert.True(forwarded.Headers.TryGetValue("X-Internal-Gateway-Secret", out var secretHeader));
        Assert.Equal(GatewayTestFactory.InternalSecret, secretHeader);

        Assert.True(forwarded.Headers.TryGetValue("X-Internal-Subject", out var subjectHeader));
        Assert.Equal("bootstrap-admin", subjectHeader);

        Assert.True(forwarded.Headers.TryGetValue("X-Internal-Email", out var emailHeader));
        Assert.Equal("admin@healthtech.local", emailHeader);
    }

    private static async Task<string> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@healthtech.local",
            Password = "devpass"
        });

        response.EnsureSuccessStatusCode();
        var cookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        Assert.False(string.IsNullOrWhiteSpace(cookie));
        return cookie!.Split(';', 2, StringSplitOptions.TrimEntries)[0];
    }

    private sealed record ErrorResponse(string Error);

    private sealed record ForwardedRequestEcho(string Path, Dictionary<string, string> Headers);

    private sealed class GatewayTestFactory(Uri backendAddress, bool withoutActiveTenant, bool allowTenantB)
        : WebApplicationFactory<Program>
    {
        public const string InternalSecret = "integration-internal-gateway-secret";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["ReverseProxy:Clusters:patients:Destinations:d1:Address"] = backendAddress.ToString(),
                    ["ReverseProxy:Clusters:appointments:Destinations:d1:Address"] = backendAddress.ToString(),
                    ["ReverseProxy:Clusters:identity:Destinations:d1:Address"] = backendAddress.ToString(),
                    ["Bff:EnableDevelopmentAuth"] = "true",
                    ["Bff:PublicBaseUrl"] = "http://localhost",
                    ["InternalGateway:SharedSecret"] = InternalSecret,
                    ["Cors:AllowedOrigins:0"] = "http://localhost",
                    ["TenantAccess:Users:0:SubjectId"] = "bootstrap-admin",
                    ["TenantAccess:Users:0:Email"] = "admin@healthtech.local",
                    ["TenantAccess:Users:0:Memberships:0:TenantId"] = TenantA.ToString("D"),
                    ["TenantAccess:Users:0:Memberships:0:TenantName"] = "Demo Clinic A",
                    ["TenantAccess:Users:0:Memberships:0:Role"] = "admin",
                    ["TenantAccess:Users:0:Memberships:0:Enabled"] = "true",
                    ["Bff:DevelopmentMemberships:0:TenantId"] = TenantA.ToString("D"),
                    ["Bff:DevelopmentMemberships:0:TenantName"] = "Demo Clinic A",
                    ["Bff:DevelopmentMemberships:0:Role"] = "admin"
                };

                if (withoutActiveTenant)
                {
                    settings["Bff:DevelopmentMemberships:1:TenantId"] = TenantB.ToString("D");
                    settings["Bff:DevelopmentMemberships:1:TenantName"] = "Demo Clinic B";
                    settings["Bff:DevelopmentMemberships:1:Role"] = "admin";
                }

                if (allowTenantB)
                {
                    settings["TenantAccess:Users:0:Memberships:1:TenantId"] = TenantB.ToString("D");
                    settings["TenantAccess:Users:0:Memberships:1:TenantName"] = "Demo Clinic B";
                    settings["TenantAccess:Users:0:Memberships:1:Role"] = "admin";
                    settings["TenantAccess:Users:0:Memberships:1:Enabled"] = "true";
                }

                config.AddInMemoryCollection(settings);
            });
        }
    }

    private sealed class BackendEchoServer(WebApplication app, Uri baseAddress) : IAsyncDisposable
    {
        public Uri BaseAddress { get; } = baseAddress;

        public static async Task<BackendEchoServer> StartAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

            var app = builder.Build();
            app.Map("/{**catchAll}", (HttpContext context) =>
            {
                var headers = context.Request.Headers.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToString(),
                    StringComparer.OrdinalIgnoreCase);

                return Results.Json(new ForwardedRequestEcho(context.Request.Path.ToString(), headers));
            });

            await app.StartAsync();
            var address = app.Urls.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new InvalidOperationException("Backend echo server did not expose a listening address.");
            }

            return new BackendEchoServer(app, new Uri(address));
        }

        public async ValueTask DisposeAsync()
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }
}

using System.Net;
using System.Net.Http;
using System.Text;
using HealthTech.BuildingBlocks.Abstractions;
using HealthTech.Gateway.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthTech.Gateway.Tests;

public sealed class BffAuthServiceTests
{
    [Fact]
    public async Task SignInWithPasswordAsync_resolves_memberships_from_tenant_access_provider()
    {
        var handler = new StubHttpMessageHandler("""
            {
              "access_token": "supabase-access-token",
              "refresh_token": "supabase-refresh-token",
              "user": {
                "id": "supabase-user-id",
                "email": "owner@clinic.local"
              }
            }
            """);

        using var client = new HttpClient(handler);
        var tenantAccessProvider = CreateTenantAccessProvider("owner@clinic.local");
        var service = new BffAuthService(
            client,
            Options.Create(new BffAuthOptions
            {
                SupabaseUrl = "https://project.supabase.co",
                SupabaseAnonKey = "anon-key"
            }),
            tenantAccessProvider,
            new TestWebHostEnvironment("Production"));

        var result = await service.SignInWithPasswordAsync(
            new BffLoginRequest("owner@clinic.local", "secret", null),
            CancellationToken.None);

        Assert.True(result.Success);
        var membership = Assert.Single(result.Memberships);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), membership.TenantId);
        Assert.Equal("Demo Clinic", membership.TenantName);
        Assert.Equal("admin", membership.Role);
    }

    [Fact]
    public async Task ExchangeOAuthCodeAsync_resolves_memberships_from_tenant_access_provider()
    {
        var handler = new StubHttpMessageHandler("""
            {
              "access_token": "supabase-access-token",
              "refresh_token": "supabase-refresh-token",
              "user": {
                "id": "supabase-google-user-id",
                "email": "owner@clinic.local"
              }
            }
            """);

        using var client = new HttpClient(handler);
        var tenantAccessProvider = CreateTenantAccessProvider("owner@clinic.local");
        var service = new BffAuthService(
            client,
            Options.Create(new BffAuthOptions
            {
                SupabaseUrl = "https://project.supabase.co",
                SupabaseAnonKey = "anon-key"
            }),
            tenantAccessProvider,
            new TestWebHostEnvironment("Production"));

        var result = await service.ExchangeOAuthCodeAsync("oauth-code", "pkce-verifier", CancellationToken.None);

        Assert.True(result.Success);
        var membership = Assert.Single(result.Memberships);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), membership.TenantId);
        Assert.Equal("Demo Clinic", membership.TenantName);
        Assert.Equal("admin", membership.Role);
    }

    [Fact]
    public async Task RequestPasswordRecoveryAsync_posts_recovery_request_to_supabase()
    {
        var handler = new StubHttpMessageHandler("{}");

        using var client = new HttpClient(handler);
        var service = new BffAuthService(
            client,
            Options.Create(new BffAuthOptions
            {
                SupabaseUrl = "https://project.supabase.co",
                SupabaseAnonKey = "anon-key"
            }),
            CreateTenantAccessProvider("owner@clinic.local"),
            new TestWebHostEnvironment("Production"));

        var result = await service.RequestPasswordRecoveryAsync("owner@clinic.local", "http://localhost/reset-password", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://project.supabase.co/auth/v1/recover", handler.LastRequest?.RequestUri?.ToString());
        Assert.Equal("anon-key", handler.LastRequest?.Headers.GetValues("apikey").Single());
    }

    private static ITenantAccessProvider CreateTenantAccessProvider(string email)
        => new BootstrapTenantAccessProvider(Options.Create(new TenantAccessOptions
        {
            Users =
            [
                new TenantAccessUserOptions
                {
                    SubjectId = "bootstrap-admin",
                    Email = email,
                    Memberships =
                    [
                        new TenantMembershipOptions
                        {
                            TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                            TenantName = "Demo Clinic",
                            Role = "admin",
                            Enabled = true
                        }
                    ]
                }
            ]
        }));

    private sealed class StubHttpMessageHandler(string json) : HttpMessageHandler
    {
        private readonly string _json = json;
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class TestWebHostEnvironment(string environmentName) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "HealthTech.Gateway.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = environmentName;
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

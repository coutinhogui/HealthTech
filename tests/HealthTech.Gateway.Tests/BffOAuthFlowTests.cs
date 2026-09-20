using HealthTech.Gateway.Security;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace HealthTech.Gateway.Tests;

public sealed class BffOAuthFlowTests
{
    [Fact]
    public void BuildAuthorizeUrl_creates_supabase_pkce_redirect()
    {
        var options = new BffAuthOptions
        {
            SupabaseUrl = "https://project.supabase.co"
        };

        var redirect = BffOAuthFlow.BuildAuthorizeUrl(
            options,
            "google",
            new Uri("https://app.healthtech.local/api/auth/callback"),
            "state-123",
            "challenge-456");

        Assert.StartsWith("https://project.supabase.co/auth/v1/authorize?", redirect);
        Assert.Contains("provider=google", redirect);
        Assert.Contains("redirect_to=https%3A%2F%2Fapp.healthtech.local%2Fapi%2Fauth%2Fcallback", redirect);
        Assert.Contains("code_challenge=challenge-456", redirect);
        Assert.Contains("code_challenge_method=s256", redirect);
        Assert.DoesNotContain("state=", redirect);
    }

    [Fact]
    public void CreateCorrelationCookieOptions_uses_short_lived_secure_http_only_cookie()
    {
        var options = BffOAuthFlow.CreateCorrelationCookieOptions();

        Assert.True(options.HttpOnly);
        Assert.True(options.Secure);
        Assert.Equal(SameSiteMode.Lax, options.SameSite);
        Assert.Equal(TimeSpan.FromMinutes(10), options.MaxAge);
        Assert.True(options.IsEssential);
    }

    [Theory]
    [InlineData("/pacientes", "/pacientes")]
    [InlineData("", "/")]
    [InlineData(null, "/")]
    [InlineData("https://evil.example/pacientes", "/")]
    [InlineData("//evil.example/pacientes", "/")]
    public void NormalizeReturnUrl_allows_only_local_paths(string? value, string expected)
    {
        Assert.Equal(expected, BffOAuthFlow.NormalizeReturnUrl(value));
    }
}

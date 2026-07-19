using HealthTech.Gateway.Security;
using Xunit;

namespace HealthTech.Gateway.Tests;

public sealed class BffAuthCapabilitiesTests
{
    [Fact]
    public void Create_hides_oauth_providers_when_supabase_is_not_configured()
    {
        var capabilities = BffAuthCapabilities.Create(new BffAuthOptions
        {
            OAuthProviders = ["google"]
        });

        Assert.False(capabilities.OAuthConfigured);
        Assert.Empty(capabilities.Providers);
    }

    [Fact]
    public void Create_exposes_oauth_providers_when_supabase_is_configured()
    {
        var capabilities = BffAuthCapabilities.Create(new BffAuthOptions
        {
            SupabaseUrl = "https://project.supabase.co",
            SupabaseAnonKey = "anon",
            OAuthProviders = ["google"]
        });

        Assert.True(capabilities.OAuthConfigured);
        Assert.Equal(["google"], capabilities.Providers);
    }

    [Fact]
    public void Create_exposes_password_fallback_only_when_development_auth_is_enabled()
    {
        var developmentCapabilities = BffAuthCapabilities.Create(new BffAuthOptions
        {
            EnableDevelopmentAuth = true
        });
        var productionCapabilities = BffAuthCapabilities.Create(new BffAuthOptions
        {
            EnableDevelopmentAuth = false,
            SupabaseUrl = "https://project.supabase.co",
            SupabaseAnonKey = "anon",
            OAuthProviders = ["google"]
        });

        Assert.True(developmentCapabilities.PasswordFallbackEnabled);
        Assert.False(productionCapabilities.PasswordFallbackEnabled);
    }
}

using HealthTech.Gateway.Security;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace HealthTech.Gateway.Tests;

public sealed class BffCookieDefaultsTests
{
    [Fact]
    public void Apply_configures_cookie_for_server_side_session()
    {
        var options = BffCookieDefaults.CreateOptions();

        Assert.True(options.Cookie.HttpOnly);
        Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
        Assert.Equal(SameSiteMode.Strict, options.Cookie.SameSite);
        Assert.Equal("/api/auth/login", options.LoginPath.Value);
        Assert.Equal("/api/auth/logout", options.LogoutPath.Value);
        Assert.Equal("HealthTech.Bff", options.Cookie.Name);
    }
}

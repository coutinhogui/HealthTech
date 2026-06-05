using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthTech.BuildingBlocks.Abstractions;

public static class InternalGatewayAuthenticationDefaults
{
    public const string Scheme = "InternalGateway";
    public const string SecretHeader = "X-Internal-Gateway-Secret";
    public const string SubjectHeader = "X-Internal-Subject";
    public const string EmailHeader = "X-Internal-Email";
}

public sealed class InternalGatewayAuthenticationOptions : AuthenticationSchemeOptions;

public sealed class InternalGatewayAuthenticationHandler(
    IOptionsMonitor<InternalGatewayAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration) : AuthenticationHandler<InternalGatewayAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var configuredSecret = configuration["InternalGateway:SharedSecret"];
        if (string.IsNullOrWhiteSpace(configuredSecret))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var providedSecret = Request.Headers[InternalGatewayAuthenticationDefaults.SecretHeader].FirstOrDefault();
        if (!string.Equals(configuredSecret, providedSecret, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var subject = Request.Headers[InternalGatewayAuthenticationDefaults.SubjectHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(subject))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing internal subject."));
        }

        var email = Request.Headers[InternalGatewayAuthenticationDefaults.EmailHeader].FirstOrDefault();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, subject),
            new("sub", subject)
        };

        if (!string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
            claims.Add(new Claim("email", email));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }
}

using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace HealthTech.Gateway.Security;

public static class BffOAuthFlow
{
    public const string CorrelationCookieName = "HealthTech.Bff.OAuth";

    public static string BuildAuthorizeUrl(
        BffAuthOptions options,
        string provider,
        Uri callbackUri,
        string state,
        string codeChallenge)
    {
        if (string.IsNullOrWhiteSpace(options.SupabaseUrl))
        {
            throw new InvalidOperationException("SupabaseUrl is required for OAuth.");
        }

        var authorizeUrl = $"{options.SupabaseUrl.TrimEnd('/')}/auth/v1/authorize";
        var query = new Dictionary<string, string?>
        {
            ["provider"] = provider,
            ["redirect_to"] = callbackUri.ToString(),
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "s256"
        };

        return QueryHelpers.AddQueryString(authorizeUrl, query);
    }

    public static CookieOptions CreateCorrelationCookieOptions()
        => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromMinutes(10),
            IsEssential = true,
            Path = "/api/auth"
        };

    public static string NormalizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) ||
            !returnUrl.StartsWith("/", StringComparison.Ordinal) ||
            returnUrl.StartsWith("//", StringComparison.Ordinal) ||
            returnUrl.IndexOf('\\') >= 0)
        {
            return "/";
        }

        return returnUrl;
    }

    public static Uri ResolveCallbackUri(HttpRequest request, string? publicBaseUrl, string? redirectTo, IReadOnlyCollection<string> allowedOrigins)
    {
        if (Uri.TryCreate(redirectTo, UriKind.Absolute, out var redirectUri) &&
            allowedOrigins.Any(origin => string.Equals(origin.TrimEnd('/'), redirectUri.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase)))
        {
            return redirectUri;
        }

        if (!string.IsNullOrWhiteSpace(publicBaseUrl) && Uri.TryCreate(publicBaseUrl.TrimEnd('/'), UriKind.Absolute, out var publicBase))
        {
            return new Uri(publicBase, "/api/auth/callback");
        }

        return new Uri($"{request.Scheme}://{request.Host}{request.PathBase}/api/auth/callback");
    }

    public static string CreateState() => CreateBase64Url(32);

    public static string CreateCodeVerifier() => CreateBase64Url(64);

    public static string CreateCodeChallenge(string verifier)
    {
        var hash = SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(verifier));
        return WebEncoders.Base64UrlEncode(hash);
    }

    private static string CreateBase64Url(int byteCount)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteCount);
        return WebEncoders.Base64UrlEncode(bytes);
    }
}

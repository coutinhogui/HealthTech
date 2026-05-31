using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace HealthTech.Gateway.Security;

public sealed class BffAuthOptions
{
    public const string SectionName = "Bff";
    public string SupabaseUrl { get; init; } = string.Empty;
    public string SupabaseAnonKey { get; init; } = string.Empty;
    public string PublicBaseUrl { get; init; } = string.Empty;
    public bool EnableDevelopmentAuth { get; init; }
    public List<string> OAuthProviders { get; init; } = ["google"];
    public List<BffTenantResponse> DevelopmentMemberships { get; init; } = [];
}

public interface IBffAuthService
{
    Task<BffSignInResult> SignInWithPasswordAsync(BffLoginRequest request, CancellationToken cancellationToken);
    Task<BffSignInResult> ExchangeOAuthCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken);
}

public sealed class BffAuthService(
    HttpClient httpClient,
    IOptions<BffAuthOptions> options,
    IWebHostEnvironment environment) : IBffAuthService
{
    public async Task<BffSignInResult> SignInWithPasswordAsync(BffLoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BffSignInResult.Failed("email_password_required");
        }

        var currentOptions = options.Value;
        if (environment.IsDevelopment() && currentOptions.EnableDevelopmentAuth)
        {
            var memberships = currentOptions.DevelopmentMemberships.Count == 0
                ? [new BffTenantResponse(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Demo Clinic", "admin")]
                : currentOptions.DevelopmentMemberships;

            return new BffSignInResult(
                true,
                null,
                "bootstrap-admin",
                request.Email.Trim(),
                null,
                null,
                memberships);
        }

        if (string.IsNullOrWhiteSpace(currentOptions.SupabaseUrl) || string.IsNullOrWhiteSpace(currentOptions.SupabaseAnonKey))
        {
            return BffSignInResult.Failed("supabase_not_configured");
        }

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"{currentOptions.SupabaseUrl.TrimEnd('/')}/auth/v1/token?grant_type=password")
        {
            Content = JsonContent.Create(new SupabasePasswordTokenRequest(request.Email.Trim(), request.Password))
        };

        message.Headers.TryAddWithoutValidation("apikey", currentOptions.SupabaseAnonKey);

        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return BffSignInResult.Failed("invalid_credentials");
        }

        var token = await response.Content.ReadFromJsonAsync<SupabasePasswordTokenResponse>(cancellationToken);
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            return BffSignInResult.Failed("invalid_supabase_response");
        }

        return new BffSignInResult(
            true,
            null,
            token.User.Id,
            token.User.Email ?? request.Email.Trim(),
            token.AccessToken,
            token.RefreshToken,
            []);
    }

    public async Task<BffSignInResult> ExchangeOAuthCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(codeVerifier))
        {
            return BffSignInResult.Failed("oauth_code_required");
        }

        var currentOptions = options.Value;
        if (string.IsNullOrWhiteSpace(currentOptions.SupabaseUrl) || string.IsNullOrWhiteSpace(currentOptions.SupabaseAnonKey))
        {
            return BffSignInResult.Failed("supabase_not_configured");
        }

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"{currentOptions.SupabaseUrl.TrimEnd('/')}/auth/v1/token?grant_type=pkce")
        {
            Content = JsonContent.Create(new SupabasePkceTokenRequest(code, codeVerifier))
        };

        message.Headers.TryAddWithoutValidation("apikey", currentOptions.SupabaseAnonKey);

        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return BffSignInResult.Failed("invalid_oauth_code");
        }

        var token = await response.Content.ReadFromJsonAsync<SupabasePasswordTokenResponse>(cancellationToken);
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            return BffSignInResult.Failed("invalid_supabase_response");
        }

        return new BffSignInResult(
            true,
            null,
            token.User.Id,
            token.User.Email,
            token.AccessToken,
            token.RefreshToken,
            []);
    }
}

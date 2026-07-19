using System.Text.Json.Serialization;

namespace HealthTech.Gateway.Security;

public sealed record BffLoginRequest(string Email, string Password, Guid? TenantId, string? AccessArea = null);

public sealed record BffRegisterRequest(string Email, string Password, Guid? TenantId, string? AccessArea = null);

public sealed record BffPasswordRecoveryRequest(string Email, string? RedirectTo);

public sealed record BffSelectTenantRequest(Guid TenantId);

public sealed record BffOAuthCorrelation(string State, string CodeVerifier, string ReturnUrl, string AccessArea);

public sealed record BffSessionResponse(
    bool Authenticated,
    string? SubjectId,
    string? Email,
    BffTenantResponse? ActiveTenant,
    IReadOnlyCollection<BffTenantResponse> Memberships,
    bool RequiresOnboarding = false,
    bool IsSystemAdmin = false,
    IReadOnlyCollection<string>? GlobalPermissions = null);

public sealed record BffTenantResponse(
    Guid TenantId,
    string TenantName,
    string Role,
    Guid? ProfessionalId = null,
    IReadOnlyCollection<string>? Permissions = null);

public sealed record BffAuthCapabilities(
    bool OAuthConfigured,
    IReadOnlyCollection<string> Providers,
    bool PasswordFallbackEnabled = false)
{
    public static BffAuthCapabilities Create(BffAuthOptions options)
    {
        var configured = !string.IsNullOrWhiteSpace(options.SupabaseUrl) &&
                         !string.IsNullOrWhiteSpace(options.SupabaseAnonKey);

        var providers = configured
            ? options.OAuthProviders
                .Where(provider => !string.IsNullOrWhiteSpace(provider))
                .Select(provider => provider.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : [];

        return new BffAuthCapabilities(configured, providers, options.EnableDevelopmentAuth);
    }
}

public sealed record BffOnboardingDecision(bool RequiresOnboarding)
{
    public static BffOnboardingDecision Create(
        bool authenticated,
        bool hasMemberships,
        bool onboardingComplete)
    {
        if (!authenticated)
        {
            return new BffOnboardingDecision(false);
        }

        return new BffOnboardingDecision(!hasMemberships || !onboardingComplete);
    }
}

public sealed record BffCompleteOnboardingRequest(
    string ClinicName,
    string ResponsibleName,
    string Phone,
    string SpecialtyName,
    string LocationName,
    string Timezone);

public sealed record BffOnboardingStatus(bool IsComplete);

public sealed record BffOnboardingCompletion(
    BffTenantResponse ActiveTenant,
    IReadOnlyCollection<BffTenantResponse> Memberships);

public sealed record BffAccessUserResponse(
    string SubjectId,
    string Email,
    string Role,
    string? FullName,
    string? Phone,
    Guid? ProfessionalId,
    bool Active);

public sealed record BffUpdateAccessUserRequest(string Role, Guid? ProfessionalId, bool Active);

public sealed record BffCreateClinicRequest(string Name, string AdminSubjectId, string AdminEmail);

public sealed record BffCreateClinicAdminRequest(string SubjectId, string Email, string? FullName, string? Phone);

public sealed record BffUpdateClinicStatusRequest(bool Active);

public sealed record BffAdminClinicResponse(Guid Id, string Name, bool Active);

public sealed record BffSystemAdminUserResponse(string SubjectId, string Email, bool Active);

public sealed record BffUpdateSystemAdminRequest(string Email, bool Active);

public sealed record BffSignInResult(
    bool Success,
    string? Error,
    string? SubjectId,
    string? Email,
    string? AccessToken,
    string? RefreshToken,
    IReadOnlyCollection<BffTenantResponse> Memberships)
{
    public static BffSignInResult Failed(string error) => new(false, error, null, null, null, null, []);
}

public sealed record BffAuthOperationResult(bool Success, string? Error)
{
    public static BffAuthOperationResult Ok() => new(true, null);
    public static BffAuthOperationResult Failed(string error) => new(false, error);
}

internal sealed record SupabasePasswordTokenRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password);

internal sealed record SupabaseSignUpRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password);

internal sealed record SupabasePasswordRecoveryRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("redirect_to")] string? RedirectTo);

internal sealed record SupabasePkceTokenRequest(
    [property: JsonPropertyName("auth_code")] string AuthCode,
    [property: JsonPropertyName("code_verifier")] string CodeVerifier);

internal sealed record SupabasePasswordTokenResponse(
    [property: JsonPropertyName("access_token")] string? AccessToken,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("user")] SupabaseUserResponse User);

internal sealed record SupabaseUserResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("email")] string? Email);

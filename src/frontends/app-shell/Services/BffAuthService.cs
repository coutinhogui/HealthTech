using System.Net.Http.Json;
using System.Security.Claims;
using HealthTech.AppShell.Auth;
using Microsoft.AspNetCore.Components;

namespace HealthTech.AppShell.Services;

public sealed class BffAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NavigationManager _navigation;
    private readonly SimpleAuthStateProvider _authState;

    public event Action? AuthStateChanged;
    public string? AvatarUrl { get; private set; }
    public string? DisplayName { get; private set; }
    public BffUser? CurrentUser { get; private set; }
    public BffTenantResponse? ActiveTenant { get; private set; }
    public IReadOnlyCollection<BffTenantResponse> Memberships { get; private set; } = [];
    public bool GoogleSignInEnabled { get; private set; }
    public bool AppleSignInEnabled { get; private set; }
    public bool PasswordFallbackEnabled { get; private set; }
    public bool RequiresOnboarding { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;

    public BffAuthService(
        IHttpClientFactory httpClientFactory,
        NavigationManager navigation,
        SimpleAuthStateProvider authState)
    {
        _httpClientFactory = httpClientFactory;
        _navigation = navigation;
        _authState = authState;
    }

    public async Task InitializeAsync()
    {
        await LoadCapabilitiesAsync();
        var session = await GetSessionAsync();
        ApplySession(session);
        AuthStateChanged?.Invoke();
    }

    public async Task<BffUser?> SignInAsync(string email, string password)
    {
        var client = _httpClientFactory.CreateClient("Bff");
        var response = await client.PostAsJsonAsync("api/auth/login", new BffLoginRequest(email, password, null));

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Nao foi possivel autenticar com as credenciais informadas.");
        }

        var session = await response.Content.ReadFromJsonAsync<BffSessionResponse>()
                      ?? throw new InvalidOperationException("Resposta de sessao invalida.");
        ApplySession(session);
        AuthStateChanged?.Invoke();
        return CurrentUser;
    }

    public async Task<BffUser?> SignUpAsync(string email, string password)
    {
        var client = _httpClientFactory.CreateClient("Bff");
        var response = await client.PostAsJsonAsync("api/auth/register", new BffRegisterRequest(email, password, null));

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Nao foi possivel criar a conta com os dados informados.");
        }

        var session = await response.Content.ReadFromJsonAsync<BffSessionResponse>()
                      ?? throw new InvalidOperationException("Resposta de sessao invalida.");
        ApplySession(session);
        AuthStateChanged?.Invoke();
        return CurrentUser;
    }

    public async Task SelectTenantAsync(Guid tenantId)
    {
        var client = _httpClientFactory.CreateClient("Bff");
        var response = await client.PostAsJsonAsync("api/session/tenant", new BffSelectTenantRequest(tenantId));

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Nao foi possivel selecionar a clinica.");
        }

        var session = await response.Content.ReadFromJsonAsync<BffSessionResponse>()
                      ?? throw new InvalidOperationException("Resposta de sessao invalida.");
        ApplySession(session);
        AuthStateChanged?.Invoke();
    }

    public async Task RequestPasswordRecoveryAsync(string email)
    {
        var client = _httpClientFactory.CreateClient("Bff");
        var redirectTo = new Uri(new Uri(_navigation.BaseUri), "login").ToString();
        var response = await client.PostAsJsonAsync("api/auth/password/recovery", new BffPasswordRecoveryRequest(email, redirectTo));

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Nao foi possivel iniciar a recuperacao de senha.");
        }
    }

    public async Task SignOutAsync()
    {
        var client = _httpClientFactory.CreateClient("Bff");
        await client.PostAsync("api/auth/logout", null);
        ApplySession(new BffSessionResponse(false, null, null, null, []));
        AuthStateChanged?.Invoke();
        _navigation.NavigateTo("/", forceLoad: false);
    }

    public Task SignInWithGoogleAsync(string? redirectTo = null)
        => SignInWithProviderAsync("google", redirectTo);

    public Task SignInWithAppleAsync(string? redirectTo = null)
        => SignInWithProviderAsync("apple", redirectTo);

    private Task SignInWithProviderAsync(string provider, string? redirectTo = null)
    {
        if (!IsProviderEnabled(provider))
        {
            throw new InvalidOperationException($"Login {provider} ainda nao esta configurado no BFF.");
        }

        var client = _httpClientFactory.CreateClient("Bff");
        if (client.BaseAddress is null)
        {
            throw new InvalidOperationException("Base URL do BFF nao configurada.");
        }

        var callbackUrl = new Uri(new Uri(_navigation.BaseUri), "auth/callback");
        var returnUrl = string.IsNullOrWhiteSpace(redirectTo) ? "/" : redirectTo;
        var loginUrl =
            new Uri(client.BaseAddress, $"api/auth/login/{Uri.EscapeDataString(provider)}?redirectTo={Uri.EscapeDataString(callbackUrl.ToString())}&returnUrl={Uri.EscapeDataString(returnUrl)}");

        _navigation.NavigateTo(loginUrl.ToString(), forceLoad: true);
        return Task.CompletedTask;
    }

    public async Task HandleOAuthRedirectAsync(Uri currentUri)
    {
        var client = _httpClientFactory.CreateClient("Bff");
        var separator = string.IsNullOrWhiteSpace(currentUri.Query) ? "?" : "&";
        var callback = "api/auth/callback" + currentUri.Query + $"{separator}responseMode=json";
        var response = await client.GetAsync(callback);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Nao foi possivel concluir o callback OAuth no BFF.");
        }

        var session = await response.Content.ReadFromJsonAsync<BffSessionResponse>() ?? await GetSessionAsync();
        ApplySession(session);
        AuthStateChanged?.Invoke();
    }

    public async Task CompleteOnboardingAsync(BffCompleteOnboardingRequest request)
    {
        var client = _httpClientFactory.CreateClient("Bff");
        var response = await client.PostAsJsonAsync("api/onboarding", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Nao foi possivel concluir a configuracao inicial.");
        }

        var session = await response.Content.ReadFromJsonAsync<BffSessionResponse>()
                      ?? throw new InvalidOperationException("Resposta de sessao invalida.");
        ApplySession(session);
        AuthStateChanged?.Invoke();
    }

    public string GetPostLoginPath()
        => RequiresOnboarding ? "/onboarding" : "/";

    public string? GetAccessToken() => null;

    private async Task<BffSessionResponse> GetSessionAsync()
    {
        var client = _httpClientFactory.CreateClient("Bff");
        return await client.GetFromJsonAsync<BffSessionResponse>("api/session")
               ?? new BffSessionResponse(false, null, null, null, []);
    }

    private async Task LoadCapabilitiesAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Bff");
            var capabilities = await client.GetFromJsonAsync<BffAuthCapabilitiesResponse>("api/auth/providers");
            GoogleSignInEnabled = capabilities?.OAuthConfigured == true &&
                                  capabilities.Providers.Any(provider => string.Equals(provider, "google", StringComparison.OrdinalIgnoreCase));
            AppleSignInEnabled = capabilities?.OAuthConfigured == true &&
                                 capabilities.Providers.Any(provider => string.Equals(provider, "apple", StringComparison.OrdinalIgnoreCase));
            PasswordFallbackEnabled = capabilities?.PasswordFallbackEnabled == true;
        }
        catch
        {
            GoogleSignInEnabled = false;
            AppleSignInEnabled = false;
            PasswordFallbackEnabled = false;
        }
    }

    private bool IsProviderEnabled(string provider)
        => string.Equals(provider, "google", StringComparison.OrdinalIgnoreCase)
            ? GoogleSignInEnabled
            : string.Equals(provider, "apple", StringComparison.OrdinalIgnoreCase) && AppleSignInEnabled;

    private void ApplySession(BffSessionResponse session)
    {
        if (!session.Authenticated)
        {
            CurrentUser = null;
            AvatarUrl = null;
            DisplayName = null;
            ActiveTenant = null;
            Memberships = [];
            RequiresOnboarding = false;
            _authState.SignOut();
            return;
        }

        CurrentUser = new BffUser(session.SubjectId ?? session.Email ?? "user", session.Email);
        ActiveTenant = session.ActiveTenant;
        Memberships = session.Memberships;
        RequiresOnboarding = session.RequiresOnboarding;
        DisplayName = session.Email?.Split('@', 2)[0] ?? "Usuario";
        AvatarUrl = null;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, CurrentUser.Id),
            new(ClaimTypes.Name, DisplayName),
        };

        if (!string.IsNullOrWhiteSpace(CurrentUser.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, CurrentUser.Email));
        }

        if (session.ActiveTenant is not null)
        {
            claims.Add(new Claim("tenant_id", session.ActiveTenant.TenantId.ToString("D")));
            claims.Add(new Claim(ClaimTypes.Role, session.ActiveTenant.Role));
        }

        _authState.SetUser(new ClaimsPrincipal(new ClaimsIdentity(claims, "BffCookie")));
    }
}

public sealed record BffUser(string Id, string? Email);
public sealed record BffLoginRequest(string Email, string Password, Guid? TenantId);
public sealed record BffRegisterRequest(string Email, string Password, Guid? TenantId);
public sealed record BffPasswordRecoveryRequest(string Email, string? RedirectTo);
public sealed record BffSelectTenantRequest(Guid TenantId);
public sealed record BffCompleteOnboardingRequest(
    string ClinicName,
    string ResponsibleName,
    string Phone,
    string SpecialtyName,
    string LocationName,
    string Timezone);
public sealed record BffTenantResponse(Guid TenantId, string TenantName, string Role);
public sealed record BffAuthCapabilitiesResponse(
    bool OAuthConfigured,
    IReadOnlyCollection<string> Providers,
    bool PasswordFallbackEnabled);
public sealed record BffSessionResponse(
    bool Authenticated,
    string? SubjectId,
    string? Email,
    BffTenantResponse? ActiveTenant,
    IReadOnlyCollection<BffTenantResponse> Memberships,
    bool RequiresOnboarding = false);

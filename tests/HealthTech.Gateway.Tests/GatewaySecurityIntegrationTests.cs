using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HealthTech.Gateway.Security;
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

    [Fact]
    public async Task Gateway_uses_internal_headers_for_service_calls_when_session_has_access_token()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(
            backend.BaseAddress,
            withoutActiveTenant: false,
            allowTenantB: true,
            authService: new AccessTokenAuthService());
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var cookie = await LoginAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/patients");
        request.Headers.TryAddWithoutValidation("Cookie", cookie);

        var response = await client.SendAsync(request);
        var forwarded = await response.Content.ReadFromJsonAsync<ForwardedRequestEcho>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(forwarded);
        Assert.True(forwarded.Headers.TryGetValue("X-Internal-Gateway-Secret", out var secretHeader));
        Assert.Equal(GatewayTestFactory.InternalSecret, secretHeader);
        Assert.True(forwarded.Headers.TryGetValue("X-Internal-Subject", out var subjectHeader));
        Assert.Equal("supabase-user-id", subjectHeader);
        Assert.False(forwarded.Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task Register_with_development_auth_creates_bff_session_with_active_tenant()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(backend.BaseAddress, withoutActiveTenant: false, allowTenantB: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "owner@clinic.local",
            Password = "devpass"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), value => value.StartsWith("HealthTech.Bff=", StringComparison.Ordinal));
        Assert.NotNull(session);
        Assert.True(session.Authenticated);
        Assert.Equal("owner@clinic.local", session.Email);
        Assert.NotNull(session.ActiveTenant);
        Assert.Equal(TenantA, session.ActiveTenant.TenantId);
    }

    [Fact]
    public async Task Discovery_clinics_are_public_without_bff_cookie()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(
            backend.BaseAddress,
            withoutActiveTenant: true,
            allowTenantB: false,
            discoveryService: new FakeDiscoveryService());
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/api/discovery/clinics");
        var clinics = await response.Content.ReadFromJsonAsync<IReadOnlyList<DiscoveryClinicResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(clinics);
        Assert.Contains(clinics, clinic => clinic.Name == "Demo Clinic A");
    }

    [Fact]
    public async Task Discovery_booking_rejects_invalid_public_request()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(
            backend.BaseAddress,
            withoutActiveTenant: true,
            allowTenantB: false,
            discoveryService: new FakeDiscoveryService());
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/api/discovery/appointments", new
        {
            TenantId = TenantA,
            ProfessionalId = Guid.Empty,
            StartsAtUtc = DateTimeOffset.UtcNow.AddDays(1),
            PatientName = "",
            PatientDocument = "",
            PatientBirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
            PatientEmail = "patient@example.com",
            PatientPhone = "11999990000"
        });
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("discovery_booking_invalid", payload?.Error);
    }

    [Fact]
    public async Task Discovery_search_is_public_and_filters_by_specialty_region_and_mode()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        var discovery = new FakeDiscoveryService();
        await using var factory = new GatewayTestFactory(
            backend.BaseAddress,
            withoutActiveTenant: true,
            allowTenantB: false,
            discoveryService: discovery);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/api/discovery/search?mode=professional&query=cardio&specialty=Cardiologia&region=Centro&take=8");
        var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<DiscoverySearchResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("Dra. Ana Cardoso", results[0].ProfessionalName);
        Assert.Equal("Cardiologia", results[0].SpecialtyName);
        Assert.Equal("Demo Clinic A", results[0].ClinicName);
        Assert.Equal("Centro, Sao Paulo - SP", results[0].RegionLabel);
        Assert.Equal("professional", discovery.LastSearchRequest?.Mode);
        Assert.Equal("cardio", discovery.LastSearchRequest?.Query);
        Assert.Equal("Cardiologia", discovery.LastSearchRequest?.Specialty);
        Assert.Equal("Centro", discovery.LastSearchRequest?.Region);
        Assert.Equal(8, discovery.LastSearchRequest?.Take);
    }

    [Fact]
    public async Task Discovery_search_accepts_browser_coordinates_for_nearby_results()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        var discovery = new FakeDiscoveryService();
        await using var factory = new GatewayTestFactory(
            backend.BaseAddress,
            withoutActiveTenant: true,
            allowTenantB: false,
            discoveryService: discovery);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/api/discovery/search?mode=professional&query=cardio&latitude=-23.561414&longitude=-46.655881&take=8");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(-23.561414, discovery.LastSearchRequest?.Latitude);
        Assert.Equal(-46.655881, discovery.LastSearchRequest?.Longitude);
    }

    [Fact]
    public async Task Discovery_slots_accept_location_id_filter()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        var discovery = new FakeDiscoveryService();
        await using var factory = new GatewayTestFactory(
            backend.BaseAddress,
            withoutActiveTenant: true,
            allowTenantB: false,
            discoveryService: discovery);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var professionalId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var locationId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var from = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(1).ToString("O"));
        var to = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(2).ToString("O"));

        var response = await client.GetAsync($"/api/discovery/slots?tenantId={TenantA:D}&professionalId={professionalId:D}&locationId={locationId:D}&fromUtc={from}&toUtc={to}&slotMinutes=30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(locationId, discovery.LastSlotLocationId);
    }

    [Fact]
    public async Task Discovery_booking_returns_not_found_when_location_does_not_belong_to_clinic()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        var discovery = new FakeDiscoveryService { ForceBookingFailure = "location_not_found" };
        await using var factory = new GatewayTestFactory(
            backend.BaseAddress,
            withoutActiveTenant: true,
            allowTenantB: false,
            discoveryService: discovery);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/api/discovery/appointments", new
        {
            TenantId = TenantA,
            ProfessionalId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            LocationId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            StartsAtUtc = DateTimeOffset.UtcNow.AddDays(1),
            EndsAtUtc = DateTimeOffset.UtcNow.AddDays(1).AddMinutes(30),
            PatientName = "Paciente Teste",
            PatientDocument = "12345678900",
            PatientBirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
            PatientEmail = "patient@example.com",
            PatientPhone = "11999990000"
        });
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("location_not_found", payload?.Error);
    }

    [Fact]
    public async Task Session_returns_active_tenant_professional_id_and_permissions()
    {
        var professionalId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(
            backend.BaseAddress,
            withoutActiveTenant: false,
            allowTenantB: true,
            role: "professional",
            professionalId: professionalId);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@healthtech.local",
            Password = "devpass"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(session?.ActiveTenant);
        Assert.Equal("professional", session.ActiveTenant.Role);
        Assert.Equal(professionalId, session.ActiveTenant.ProfessionalId);
        Assert.Contains("ReadOwnClinicalSchedule", session.ActiveTenant.Permissions);
        Assert.DoesNotContain("ManageBilling", session.ActiveTenant.Permissions);
    }

    [Fact]
    public async Task Session_returns_system_admin_global_permissions_without_active_tenant_requirement()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(
            backend.BaseAddress,
            withoutActiveTenant: true,
            allowTenantB: false,
            systemAdmin: true,
            includeDevelopmentMemberships: false,
            authService: new NoMembershipAuthService());
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "sysadmin@healthtech.local",
            Password = "devpass",
            AccessArea = "environment"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(session);
        Assert.True(session.IsSystemAdmin);
        Assert.Null(session.ActiveTenant);
        Assert.Empty(session.Memberships);
        Assert.Contains("ManageClinics", session.GlobalPermissions);
        Assert.Contains("ManageSystemAdmins", session.GlobalPermissions);
        Assert.False(session.RequiresOnboarding);
    }

    [Fact]
    public async Task Select_tenant_updates_active_tenant_cookie_for_existing_membership()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(backend.BaseAddress, withoutActiveTenant: true, allowTenantB: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var initialCookie = await LoginAsync(client);
        using var selectRequest = new HttpRequestMessage(HttpMethod.Post, "/api/session/tenant")
        {
            Content = JsonContent.Create(new { TenantId = TenantB })
        };
        selectRequest.Headers.TryAddWithoutValidation("Cookie", initialCookie);

        var selectResponse = await client.SendAsync(selectRequest);

        Assert.Equal(HttpStatusCode.OK, selectResponse.StatusCode);
        var selectedSession = await selectResponse.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(selectedSession);
        Assert.Equal(TenantB, selectedSession.ActiveTenant?.TenantId);

        var selectedCookie = BuildCookieHeader(selectResponse);
        using var sessionRequest = new HttpRequestMessage(HttpMethod.Get, "/api/session");
        sessionRequest.Headers.TryAddWithoutValidation("Cookie", selectedCookie);
        var sessionResponse = await client.SendAsync(sessionRequest);
        var cookieSession = await sessionResponse.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.Equal(TenantB, cookieSession?.ActiveTenant?.TenantId);

        using var routeRequest = new HttpRequestMessage(HttpMethod.Get, "/api/patients");
        routeRequest.Headers.TryAddWithoutValidation("Cookie", selectedCookie);

        var routeResponse = await client.SendAsync(routeRequest);
        var routeBody = await routeResponse.Content.ReadAsStringAsync();

        Assert.True(routeResponse.StatusCode == HttpStatusCode.OK, routeBody);
        var forwarded = System.Text.Json.JsonSerializer.Deserialize<ForwardedRequestEcho>(routeBody, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(forwarded);
        Assert.Equal(TenantB.ToString("D"), forwarded.Headers["X-Tenant-Id"]);
    }

    [Fact]
    public async Task Complete_onboarding_is_forbidden_when_public_onboarding_is_disabled()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        var onboarding = new FakeOnboardingService();
        await using var factory = new GatewayTestFactory(
            backend.BaseAddress,
            withoutActiveTenant: true,
            allowTenantB: true,
            onboarding);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var cookie = await LoginAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/onboarding")
        {
            Content = JsonContent.Create(new
            {
                ClinicName = "Clinica Coutinho",
                ResponsibleName = "Guilherme Coutinho",
                Phone = "+55 11 99999-0000",
                SpecialtyName = "Clinica geral",
                LocationName = "Unidade principal",
                Timezone = "America/Sao_Paulo"
            })
        };
        request.Headers.TryAddWithoutValidation("Cookie", cookie);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(onboarding.LastRequest);
    }

    [Fact]
    public async Task System_admin_can_create_clinic_and_first_admin()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(
            backend.BaseAddress,
            withoutActiveTenant: true,
            allowTenantB: false,
            systemAdmin: true,
            includeDevelopmentMemberships: false,
            authService: new NoMembershipAuthService());
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var cookie = await LoginAsync(client, "sysadmin@healthtech.local", "environment");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/clinics")
        {
            Content = JsonContent.Create(new
            {
                Name = "Clinica SaaS",
                AdminSubjectId = "clinic-admin-sub",
                AdminEmail = "admin@clinic.local"
            })
        };
        request.Headers.TryAddWithoutValidation("Cookie", cookie);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var clinic = await response.Content.ReadFromJsonAsync<AdminClinicResponse>();
        Assert.NotNull(clinic);
        Assert.Equal("Clinica SaaS", clinic.Name);
        Assert.True(clinic.Active);
    }

    [Fact]
    public async Task Clinic_admin_cannot_call_global_admin_endpoints()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(backend.BaseAddress, withoutActiveTenant: false, allowTenantB: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var cookie = await LoginAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/clinics");
        request.Headers.TryAddWithoutValidation("Cookie", cookie);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Clinic_admin_cannot_login_through_environment_access()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(backend.BaseAddress, withoutActiveTenant: false, allowTenantB: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@healthtech.local",
            Password = "devpass",
            AccessArea = "environment"
        });
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("system_admin_required", payload?.Error);
    }

    [Fact]
    public async Task System_admin_without_clinic_membership_cannot_login_through_clinic_access()
    {
        await using var backend = await BackendEchoServer.StartAsync();
        await using var factory = new GatewayTestFactory(
            backend.BaseAddress,
            withoutActiveTenant: true,
            allowTenantB: false,
            systemAdmin: true,
            includeDevelopmentMemberships: false,
            authService: new NoMembershipAuthService());
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "sysadmin@healthtech.local",
            Password = "devpass",
            AccessArea = "clinic"
        });
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("clinic_membership_required", payload?.Error);
    }

    private static string BuildCookieHeader(HttpResponseMessage response)
        => string.Join("; ", response.Headers.GetValues("Set-Cookie")
            .Where(value => value.StartsWith("HealthTech.Bff", StringComparison.Ordinal))
            .Select(value => value.Split(';', 2, StringSplitOptions.TrimEntries)[0]));

    private static Task<string> LoginAsync(HttpClient client)
        => LoginAsync(client, "admin@healthtech.local");

    private static async Task<string> LoginAsync(HttpClient client, string email)
        => await LoginAsync(client, email, "clinic");

    private static async Task<string> LoginAsync(HttpClient client, string email, string accessArea)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = "devpass",
            AccessArea = accessArea
        });

        response.EnsureSuccessStatusCode();
        var cookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        Assert.False(string.IsNullOrWhiteSpace(cookie));
        return cookie!.Split(';', 2, StringSplitOptions.TrimEntries)[0];
    }

    private sealed record ErrorResponse(string Error);

    private sealed record SessionResponse(
        bool Authenticated,
        string? SubjectId,
        string? Email,
        TenantResponse? ActiveTenant,
        IReadOnlyCollection<TenantResponse> Memberships,
        bool RequiresOnboarding,
        bool IsSystemAdmin,
        IReadOnlyCollection<string> GlobalPermissions);

    private sealed record TenantResponse(
        Guid TenantId,
        string TenantName,
        string Role,
        Guid? ProfessionalId,
        IReadOnlyCollection<string> Permissions);

    private sealed record AdminClinicResponse(Guid Id, string Name, bool Active);
    private sealed record DiscoveryClinicResponse(Guid Id, string Name, bool Active);
    private sealed record DiscoverySearchResponse(
        Guid TenantId,
        Guid ProfessionalId,
        Guid? LocationId,
        string ProfessionalName,
        string SpecialtyName,
        string ClinicName,
        string? LocationName,
        string? City,
        string? State,
        string RegionLabel,
        IReadOnlyCollection<BffDiscoverySlotResponse> NextSlots);

    private sealed record ForwardedRequestEcho(string Path, Dictionary<string, string> Headers);

    private sealed class GatewayTestFactory(
        Uri backendAddress,
        bool withoutActiveTenant,
        bool allowTenantB,
        IBffOnboardingService? onboardingService = null,
        IBffAuthService? authService = null,
        IBffDiscoveryService? discoveryService = null,
        string role = "admin",
        Guid? professionalId = null,
        bool systemAdmin = false,
        bool includeDevelopmentMemberships = true)
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
                    ["TenantAccess:Users:0:Memberships:0:Role"] = role,
                    ["TenantAccess:Users:0:Memberships:0:Enabled"] = "true",
                };

                if (includeDevelopmentMemberships)
                {
                    settings["Bff:DevelopmentMemberships:0:TenantId"] = TenantA.ToString("D");
                    settings["Bff:DevelopmentMemberships:0:TenantName"] = "Demo Clinic A";
                    settings["Bff:DevelopmentMemberships:0:Role"] = role;
                }
                else
                {
                    settings["Bff:EnableDefaultDevelopmentMembership"] = "false";
                }

                if (systemAdmin)
                {
                    settings["SystemAdmin:Users:0:SubjectId"] = "bootstrap-admin";
                    settings["SystemAdmin:Users:0:Email"] = "sysadmin@healthtech.local";
                    settings["SystemAdmin:Users:0:Active"] = "true";
                }

                if (professionalId is not null)
                {
                    settings["TenantAccess:Users:0:Memberships:0:ProfessionalId"] = professionalId.Value.ToString("D");
                    if (includeDevelopmentMemberships)
                    {
                        settings["Bff:DevelopmentMemberships:0:ProfessionalId"] = professionalId.Value.ToString("D");
                    }
                }

                if (withoutActiveTenant)
                {
                    if (includeDevelopmentMemberships)
                    {
                        settings["Bff:DevelopmentMemberships:1:TenantId"] = TenantB.ToString("D");
                        settings["Bff:DevelopmentMemberships:1:TenantName"] = "Demo Clinic B";
                        settings["Bff:DevelopmentMemberships:1:Role"] = role;
                    }
                }

                if (allowTenantB)
                {
                    settings["TenantAccess:Users:0:Memberships:1:TenantId"] = TenantB.ToString("D");
                    settings["TenantAccess:Users:0:Memberships:1:TenantName"] = "Demo Clinic B";
                    settings["TenantAccess:Users:0:Memberships:1:Role"] = role;
                    settings["TenantAccess:Users:0:Memberships:1:Enabled"] = "true";
                }

                config.AddInMemoryCollection(settings);
            });

            if (onboardingService is not null)
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IBffOnboardingService>();
                    services.AddSingleton(onboardingService);
                });
            }

            if (authService is not null)
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IBffAuthService>();
                    services.AddSingleton(authService);
                });
            }

            if (discoveryService is not null)
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IBffDiscoveryService>();
                    services.AddSingleton(discoveryService);
                });
            }
        }
    }

    private sealed class AccessTokenAuthService : IBffAuthService
    {
        public Task<BffSignInResult> SignInWithPasswordAsync(BffLoginRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new BffSignInResult(
                true,
                null,
                "supabase-user-id",
                request.Email,
                "supabase-access-token",
                "supabase-refresh-token",
                [new BffTenantResponse(TenantA, "Demo Clinic A", "admin", null, [])]));

        public Task<BffSignInResult> RegisterWithPasswordAsync(BffRegisterRequest request, CancellationToken cancellationToken)
            => SignInWithPasswordAsync(new BffLoginRequest(request.Email, request.Password, request.TenantId), cancellationToken);

        public Task<BffAuthOperationResult> RequestPasswordRecoveryAsync(string email, string? redirectTo, CancellationToken cancellationToken)
            => Task.FromResult(BffAuthOperationResult.Ok());

        public Task<BffSignInResult> ExchangeOAuthCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken)
            => Task.FromResult(new BffSignInResult(
                true,
                null,
                "supabase-user-id",
                "owner@clinic.local",
                "supabase-access-token",
                "supabase-refresh-token",
                [new BffTenantResponse(TenantA, "Demo Clinic A", "admin", null, [])]));
    }

    private sealed class NoMembershipAuthService : IBffAuthService
    {
        public Task<BffSignInResult> SignInWithPasswordAsync(BffLoginRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new BffSignInResult(
                true,
                null,
                "bootstrap-admin",
                request.Email,
                null,
                null,
                []));

        public Task<BffSignInResult> RegisterWithPasswordAsync(BffRegisterRequest request, CancellationToken cancellationToken)
            => SignInWithPasswordAsync(new BffLoginRequest(request.Email, request.Password, request.TenantId), cancellationToken);

        public Task<BffAuthOperationResult> RequestPasswordRecoveryAsync(string email, string? redirectTo, CancellationToken cancellationToken)
            => Task.FromResult(BffAuthOperationResult.Ok());

        public Task<BffSignInResult> ExchangeOAuthCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken)
            => Task.FromResult(new BffSignInResult(true, null, "bootstrap-admin", "sysadmin@healthtech.local", null, null, []));
    }

    private sealed class FakeDiscoveryService : IBffDiscoveryService
    {
        public BffDiscoverySearchRequest? LastSearchRequest { get; private set; }
        public Guid? LastSlotLocationId { get; private set; }
        public string? ForceBookingFailure { get; init; }

        public Task<IReadOnlyCollection<BffDiscoverySearchResponse>> SearchAsync(
            BffDiscoverySearchRequest request,
            CancellationToken cancellationToken)
        {
            LastSearchRequest = request;
            return Task.FromResult<IReadOnlyCollection<BffDiscoverySearchResponse>>(
            [
                new BffDiscoverySearchResponse(
                    TenantA,
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    "Dra. Ana Cardoso",
                    "Cardiologia",
                    "Demo Clinic A",
                    "Unidade Centro",
                    "Sao Paulo",
                    "SP",
                    "Centro, Sao Paulo - SP",
                    [new BffDiscoverySlotResponse(
                        Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        DateTimeOffset.UtcNow.AddDays(1),
                        DateTimeOffset.UtcNow.AddDays(1).AddMinutes(30))])
            ]);
        }

        public Task<IReadOnlyCollection<BffDiscoveryClinicResponse>> ListClinicsAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<BffDiscoveryClinicResponse>>(
            [
                new BffDiscoveryClinicResponse(TenantA, "Demo Clinic A", true)
            ]);

        public Task<IReadOnlyCollection<BffDiscoverySpecialtyResponse>> ListSpecialtiesAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<BffDiscoverySpecialtyResponse>>([]);

        public Task<IReadOnlyCollection<BffDiscoveryProfessionalResponse>> ListProfessionalsAsync(Guid tenantId, Guid? specialtyId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<BffDiscoveryProfessionalResponse>>([]);

        public Task<IReadOnlyCollection<BffDiscoverySlotResponse>> ListSlotsAsync(
            Guid tenantId,
            Guid professionalId,
            Guid? locationId,
            DateTimeOffset fromUtc,
            DateTimeOffset toUtc,
            int slotMinutes,
            CancellationToken cancellationToken)
        {
            LastSlotLocationId = locationId;
            return Task.FromResult<IReadOnlyCollection<BffDiscoverySlotResponse>>([]);
        }

        public Task<BffDiscoveryBookingResult> BookAsync(BffDiscoveryBookingRequest request, CancellationToken cancellationToken)
            => Task.FromResult(ForceBookingFailure is null
                ? BffDiscoveryBookingResult.Ok(Guid.NewGuid())
                : BffDiscoveryBookingResult.Fail(ForceBookingFailure, "Location not found."));
    }

    private sealed class FakeOnboardingService : IBffOnboardingService
    {
        public static readonly Guid CreatedTenantId = Guid.Parse("aaaaaaaa-1111-4444-8888-000000000001");
        public BffCompleteOnboardingRequest? LastRequest { get; private set; }

        public Task<BffOnboardingStatus> GetStatusAsync(
            string subjectId,
            string? email,
            BffTenantResponse? activeTenant,
            IReadOnlyCollection<BffTenantResponse> memberships,
            CancellationToken cancellationToken)
            => Task.FromResult(new BffOnboardingStatus(memberships.Count > 0));

        public Task<BffOnboardingCompletion> CompleteAsync(
            string subjectId,
            string? email,
            BffCompleteOnboardingRequest request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            var tenant = new BffTenantResponse(CreatedTenantId, request.ClinicName, "admin", null, []);
            return Task.FromResult(new BffOnboardingCompletion(tenant, [tenant]));
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

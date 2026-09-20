using HealthTech.Front;
using HealthTech.Front.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.AspNetCore.Components.Authorization;
using HealthTech.Front.Auth; 
using HealthTech.Front.Features.Appointment;
using HealthTech.Front.Features.Billing;
using HealthTech.Front.Features.Discovery;
using HealthTech.Front.Features.Ehr;
using HealthTech.Front.Features.Patient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();

// Serviços
builder.Services.AddScoped<SimpleAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SimpleAuthStateProvider>());
builder.Services.AddScoped<BffAuthService>();
builder.Services.AddScoped<CookieCredentialsMessageHandler>();
builder.Services.AddScoped<TenantAccessRevocationHandler>();
builder.Services.AddScoped(sp => new PatientBffClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bff")));
builder.Services.AddScoped(sp => new AppointmentBffClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bff")));
builder.Services.AddScoped(sp => new DiscoveryBffClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bff")));
builder.Services.AddScoped(sp => new AppointmentPatientOptionsClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bff")));
builder.Services.AddScoped(sp => new AppointmentProfessionalOptionsClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bff")));
builder.Services.AddScoped(sp => new ProfessionalManagementBffClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bff")));
builder.Services.AddScoped(sp => new EhrBffClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bff")));
builder.Services.AddScoped(sp => new BillingBffClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bff")));
builder.Services.AddScoped(sp => new BillingPatientOptionsClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bff")));
builder.Services.AddScoped(sp => new BillingAppointmentOptionsClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bff")));

builder.Services.AddMudServices();


builder.Services.AddHttpClient("Bff", client =>
{
    client.BaseAddress = new Uri(ResolveApiBaseUrl());
})
    .AddHttpMessageHandler<CookieCredentialsMessageHandler>()
    .AddHttpMessageHandler<TenantAccessRevocationHandler>();

builder.Services.AddHttpClient("IdentityApi", client =>
{
    client.BaseAddress = new Uri(ResolveApiBaseUrl());
}).AddHttpMessageHandler<CookieCredentialsMessageHandler>();

// HttpClient “padrão” (útil para chamadas ao mesmo host do app)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

await builder.Build().RunAsync();

string ResolveApiBaseUrl()
{
    var configuredBaseUrl = builder.Configuration["Apis:Bff"];
    return string.IsNullOrWhiteSpace(configuredBaseUrl) || configuredBaseUrl == "/"
        ? builder.HostEnvironment.BaseAddress
        : configuredBaseUrl;
}

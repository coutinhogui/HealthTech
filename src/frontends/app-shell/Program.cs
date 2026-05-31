using HealthTech.AppShell;
using HealthTech.AppShell.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.AspNetCore.Components.Authorization;
using HealthTech.AppShell.Auth; 
using HealthTech.Frontends.MfAppointment;
using HealthTech.Frontends.MfBilling;
using HealthTech.Frontends.MfEhr;
using HealthTech.Frontends.MfPatient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();

// Serviços
builder.Services.AddScoped<SimpleAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SimpleAuthStateProvider>());
builder.Services.AddScoped<BffAuthService>();
builder.Services.AddScoped<CookieCredentialsMessageHandler>();
builder.Services.AddScoped(sp => new PatientBffClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bff")));
builder.Services.AddScoped(sp => new AppointmentBffClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Bff")));
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
    var baseUrl = builder.Configuration["Apis:Bff"] ?? builder.HostEnvironment.BaseAddress;
    client.BaseAddress = new Uri(baseUrl!);
}).AddHttpMessageHandler<CookieCredentialsMessageHandler>();

builder.Services.AddHttpClient("IdentityApi", client =>
{
    var baseUrl = builder.Configuration["Apis:Bff"] ?? builder.HostEnvironment.BaseAddress;
    client.BaseAddress = new Uri(baseUrl!);
}).AddHttpMessageHandler<CookieCredentialsMessageHandler>();

// HttpClient “padrão” (útil para chamadas ao mesmo host do app)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

await builder.Build().RunAsync();

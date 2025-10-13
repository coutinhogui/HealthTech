using HealthTech.AppShell;
using HealthTech.AppShell.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.AspNetCore.Components.Authorization;
using HealthTech.AppShell.Auth; 

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();

// Serviços
builder.Services.AddScoped<SimpleAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SimpleAuthStateProvider>());
builder.Services.AddScoped<SupabaseAuthService>();
builder.Services.AddScoped<BearerAuthorizationMessageHandler>();
builder.Services.AddScoped<BlazorLocalStorageSessionPersistence>();

builder.Services.AddMudServices();


// HttpClient nomeado para a Identity API (lê URL de appsettings.json)
builder.Services.AddHttpClient("IdentityApi", client =>
{
    var baseUrl = builder.Configuration["Apis:Identity"];
    client.BaseAddress = new Uri(baseUrl!);
});

// HttpClient “padrão” (útil para chamadas ao mesmo host do app)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

await builder.Build().RunAsync();

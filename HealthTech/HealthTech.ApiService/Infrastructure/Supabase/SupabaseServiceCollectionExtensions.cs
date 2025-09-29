using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Supabase;

namespace HealthTech.ApiService.Infrastructure.Supabase;

public static class SupabaseServiceCollectionExtensions
{
    public static IServiceCollection AddSupabaseClient(this IServiceCollection services, IConfiguration config)
    {
        // Bind de configurações
        services.AddOptions<SupabaseSettings>()
            .Bind(config.GetSection("Supabase"))
            .ValidateDataAnnotations()
            .Validate(s => !string.IsNullOrWhiteSpace(s.Url) && !string.IsNullOrWhiteSpace(s.ServiceKey),
                "Supabase Url/ServiceKey inválidos.");

        // Registro do cliente como Singleton (thread-safe p/ reuso de HttpClient interno)
        services.AddSingleton(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<SupabaseSettings>>().Value;

            var options = new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true                    
                    // SessionHandler = new SupabaseSessionHandler() <-- This must be implemented by the developer
                };

            // Cria o cliente
            var client = new Client(settings.Url, settings.ServiceKey, options);

            // Se você precisar inicializar canais Realtime imediatamente:
            // await client.InitializeAsync();  // (não bloqueie aqui; faça on-demand/Startup BackgroundService se necessário)

            return client;
        });

        return services;
    }
}

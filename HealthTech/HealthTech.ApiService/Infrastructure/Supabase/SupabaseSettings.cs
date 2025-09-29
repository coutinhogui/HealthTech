using System.ComponentModel.DataAnnotations;

namespace HealthTech.ApiService.Infrastructure.Supabase;

public sealed class SupabaseSettings
{
    [Required, Url]
    public string Url { get; init; } = default!;

    [Required] // use a service_role key aqui (apenas no servidor)
    public string ServiceKey { get; init; } = default!;
}

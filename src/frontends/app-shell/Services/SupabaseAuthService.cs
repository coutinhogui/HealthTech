using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Supabase.Gotrue;                  // User/Session
using SbClient = Supabase.Client;       // evita ambiguidade com Gotrue.Client

namespace HealthTech.AppShell.Services
{
    public class SupabaseAuthService
    {
        private readonly SbClient _client;

        public SupabaseAuthService(IConfiguration config)
        {
            var url = config["Supabase:Url"]!;
            var key = config["Supabase:AnonKey"]!;

            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true
                // PersistSession foi removido nas versões 1.x
            };

            _client = new SbClient(url, key, options);
        }

        public async Task<User?> SignInAsync(string email, string password)
        {
            await _client.InitializeAsync();

            // Na 1.1.1 este método existe:
            var session = await _client.Auth.SignIn(email, password);

            // (se seu intellisense mostrar SignInWithPassword(email, password), pode usar esse nome)
            return session?.User;
        }

        public async Task SignOutAsync() => await _client.Auth.SignOut();

        public string? GetJwt() => _client.Auth.CurrentSession?.AccessToken;

        public User? CurrentUser() => _client.Auth.CurrentUser;
    }
}

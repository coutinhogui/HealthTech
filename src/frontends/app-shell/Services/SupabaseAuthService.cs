using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;          // QueryHelpers.ParseQuery
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Supabase.Gotrue;

namespace HealthTech.AppShell.Services
{
    /// <summary>
    /// Autenticação usando diretamente o GoTrue (Supabase.Gotrue.Client).
    /// Suporta: Email/Senha, Logout e Google (OAuth PKCE).
    /// </summary>
    public class SupabaseAuthService
    {
        private readonly NavigationManager _nav;
        private readonly IJSRuntime _js;
        private readonly Client _auth; // GoTrue client
        private readonly string _siteUrl; // base do app (para RedirectTo)
        

        public SupabaseAuthService(IConfiguration config, IJSRuntime js, NavigationManager nav)
        {
            _nav = nav;
            _js = js;

            var projectUrl = config["Supabase:Url"]; // ex: https://xxxx.supabase.co
            var anonKey = config["Supabase:Key"]; // anon ou service_role (NÃO usar service_role no browser)

            if (string.IsNullOrWhiteSpace(projectUrl) || string.IsNullOrWhiteSpace(anonKey))
                throw new InvalidOperationException("Supabase Url/Key ausente no appsettings.json.");

            // GoTrue usa o endpoint /auth/v1
            var authUrl = projectUrl!.TrimEnd('/') + "/auth/v1";

            var options = new Supabase.Gotrue.ClientOptions
            {
                Url = authUrl,
                AutoRefreshToken = true,            // renova tokens automaticamente
            };
            // Headers necessários para a Auth API:
            options.Headers["apikey"] = anonKey!;
            options.Headers["Authorization"] = $"Bearer {anonKey!}";

            _auth = new Client(options);

            // Persistência da sessão no localStorage (sua implementação):
            _auth.SetPersistence(new BlazorLocalStorageSessionPersistence(js));

            // tenta carregar sessão que já exista (não é async)
            _auth.LoadSession();

            // base URL do seu app para construir o RedirectTo do OAuth
            _siteUrl = _nav.BaseUri.TrimEnd('/');
        }

        // --------- Email & Senha ---------

        public async Task<User?> SignUpAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Email e senha são obrigatórios.");

            var session = await _auth.SignUp(email.Trim(), password.Trim());
            return session?.User ?? _auth.CurrentUser;   // se confirmação de e-mail estiver ativa, session pode vir null
        }

        public async Task<User?> SignInAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Email e senha são obrigatórios.");

            var session = await _auth.SignInWithPassword(email.Trim(), password.Trim());
            return session?.User;
        }

        public async Task SignOutAsync()
        {
            // invalida as sessões do usuário no servidor e limpa a persistência local
            await _auth.SignOut(); // escopo Global por padrão
        }

        public string? GetAccessToken() => _auth.CurrentSession?.AccessToken;
        public User? CurrentUser => _auth.CurrentUser;
        public bool IsAuthenticated => _auth.CurrentUser is not null;

        // --------- Google OAuth (PKCE) ---------
        //
        // Fluxo recomendado pela lib: primeiro chame SignIn(provider, options) (PKCE),
        // ele retorna um ProviderAuthState com Uri e PKCEVerifier; você redireciona
        // o usuário para state.Uri e guarda state.PKCEVerifier. No callback, troque
        // ?code pelo token chamando ExchangeCodeForSession(verifier, code). :contentReference[oaicite:1]{index=1}

        public async Task SignInWithGoogleAsync(string? redirectTo = null)
        {
            // rota do seu app que processa o callback (ex.: /auth/callback)
            redirectTo ??= $"{_siteUrl}/auth/callback";

            var state = await _auth.SignIn(
                Supabase.Gotrue.Constants.Provider.Google,
                new SignInOptions {
                    FlowType   = Supabase.Gotrue.Constants.OAuthFlowType.PKCE,
                    RedirectTo = redirectTo
                });

            // guarde o verifier para usar no callback
            await _js.InvokeVoidAsync("localStorage.setItem", "supabase.pkce.verifier", state.PKCEVerifier);

            // redireciona para o provedor
            _nav.NavigateTo(state.Uri.ToString(), forceLoad: true);
        }

        /// <summary>
        /// Deve ser chamado na página /auth/callback (após o redirect do Supabase/Google).
        /// </summary>
        public async Task HandleOAuthRedirectAsync(Uri currentUri)
        {
            var query = QueryHelpers.ParseQuery(currentUri.Query);

            if (query.TryGetValue("code", out var codeValues))
            {
                var authCode = codeValues.ToString();
                var verifier = await _js.InvokeAsync<string>("localStorage.getItem", "supabase.pkce.verifier");

                if (string.IsNullOrWhiteSpace(verifier))
                    throw new InvalidOperationException("PKCE verifier não encontrado no localStorage.");

                // troca code + verifier por sessão do usuário
                await _auth.ExchangeCodeForSession(verifier, authCode); // :contentReference[oaicite:3]{index=3}

                // limpeza
                await _js.InvokeVoidAsync("localStorage.removeItem", "supabase.pkce.verifier");
            }
            else if (query.TryGetValue("error_description", out var err))
            {
                throw new Exception($"OAuth error: {err}");
            }
        }
    }
}

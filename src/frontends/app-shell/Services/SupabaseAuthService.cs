using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;          // QueryHelpers.ParseQuery
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Supabase.Gotrue;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using HealthTech.AppShell.Auth; // <- onde está o SimpleAuthStateProvider

namespace HealthTech.AppShell.Services
{
    /// <summary>
    /// Autenticação via GoTrue (Supabase.Gotrue.Client).
    /// Integra com SimpleAuthStateProvider para acionar <AuthorizeView>.
    /// Suporta: Email/Senha, Logout e Google (OAuth PKCE).
    /// </summary>
    public class SupabaseAuthService
    {
        private readonly NavigationManager _nav;
        private readonly IJSRuntime _js;
        private readonly Client _auth;             // GoTrue client
        private readonly string _siteUrl;          // base do app (para RedirectTo)
        private readonly SimpleAuthStateProvider _authState; // <-- NOVO: provider usado pelo Blazor

        // --- Expor dados para a UI (AppBar) ---
        public event Action? AuthStateChanged;     // para StateHasChanged no layout
        public string? AvatarUrl { get; private set; }
        public string? DisplayName { get; private set; }

        public SupabaseAuthService(
            IConfiguration config,
            IJSRuntime js,
            NavigationManager nav,
            SimpleAuthStateProvider authState)     // <-- injeta o provider
        {
            _nav = nav;
            _js = js;
            _authState = authState;

            var projectUrl = config["Supabase:Url"]; // ex: https://xxxx.supabase.co
            var anonKey = config["Supabase:Key"];    // anon (NÃO usar service_role no browser)

            if (string.IsNullOrWhiteSpace(projectUrl) || string.IsNullOrWhiteSpace(anonKey))
                throw new InvalidOperationException("Supabase Url/Key ausente no appsettings.json.");

            // GoTrue usa o endpoint /auth/v1
            var authUrl = projectUrl!.TrimEnd('/') + "/auth/v1";

            var options = new Supabase.Gotrue.ClientOptions
            {
                Url = authUrl,
                AutoRefreshToken = true,
            };
            options.Headers["apikey"] = anonKey!;
            options.Headers["Authorization"] = $"Bearer {anonKey!}";

            _auth = new Client(options);

            // Persistência da sessão no localStorage (sua implementação):
            _auth.SetPersistence(new BlazorLocalStorageSessionPersistence(js));

            // tenta carregar sessão que já exista (não é async)
            _auth.LoadSession();

            // base URL do seu app para construir o RedirectTo do OAuth
            _siteUrl = _nav.BaseUri.TrimEnd('/');

            // Popular UI/Provider já na inicialização (se houver sessão)
            ApplyAuthFromCurrentSession();               // <-- NOVO
        }

        // --------- Email & Senha ---------

        public async Task<User?> SignUpAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Email e senha são obrigatórios.");

            var session = await _auth.SignUp(email.Trim(), password.Trim());

            // Quando confirmação por email está habilitada, pode não vir sessão ainda.
            // Mesmo assim, atualize UI com CurrentUser (se setado) ou deixe anônimo.
            UpdateProfileFromUser(session?.User ?? _auth.CurrentUser);
            ApplyAuthFromCurrentSession();
            AuthStateChanged?.Invoke();

            return session?.User ?? _auth.CurrentUser;
        }

        public async Task<User?> SignInAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Email e senha são obrigatórios.");

            var session = await _auth.SignInWithPassword(email.Trim(), password.Trim());

            UpdateProfileFromUser(session?.User ?? _auth.CurrentUser);
            ApplyAuthFromCurrentSession();   // <-- sobe ClaimsPrincipal
            AuthStateChanged?.Invoke();

            return session?.User;
        }

        public async Task SignOutAsync()
        {
            // invalida as sessões do usuário no servidor e limpa a persistência local
            await _auth.SignOut(); // escopo Global por padrão

            // limpar dados de UI
            AvatarUrl = null;
            DisplayName = null;

            // derruba o principal no Blazor
            _authState.SignOut();            // <-- NOVO

            AuthStateChanged?.Invoke();
        }

        public string? GetAccessToken() => _auth.CurrentSession?.AccessToken;
        public User? CurrentUser => _auth.CurrentUser;
        public bool IsAuthenticated => _auth.CurrentUser is not null;

        // --------- Google OAuth (PKCE) ---------
        public async Task SignInWithGoogleAsync(string? redirectTo = null)
        {
            // rota do seu app que processa o callback (ex.: /auth/callback)
            redirectTo ??= $"{_siteUrl}/auth/callback";

            var state = await _auth.SignIn(
                Supabase.Gotrue.Constants.Provider.Google,
                new SignInOptions
                {
                    FlowType = Supabase.Gotrue.Constants.OAuthFlowType.PKCE,
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
                await _auth.ExchangeCodeForSession(verifier, authCode);

                // limpeza
                await _js.InvokeVoidAsync("localStorage.removeItem", "supabase.pkce.verifier");

                // Atualiza dados para a UI e provider
                UpdateProfileFromUser(_auth.CurrentUser);
                ApplyAuthFromCurrentSession();   // <-- NOVO
                AuthStateChanged?.Invoke();
            }
            else if (query.TryGetValue("error_description", out var err))
            {
                throw new Exception($"OAuth error: {err}");
            }
        }

        // ----------------------- Integração com AuthenticationState -----------------------

        /// <summary>
        /// Aplica o usuário atual do Supabase ao AuthenticationStateProvider.
        /// </summary>
        private void ApplyAuthFromCurrentSession()
        {
            var user = _auth.CurrentUser;
            if (user is null)
            {
                _authState.SignOut();
                return;
            }

            var principal = BuildPrincipal(user);
            _authState.SetUser(principal);
        }

        /// <summary>
        /// Constrói um ClaimsPrincipal a partir do Supabase.Gotrue.User
        /// </summary>
        private static ClaimsPrincipal BuildPrincipal(User user)
        {
            var claims = new List<Claim>();

            // Ids básicos
            if (!string.IsNullOrWhiteSpace(user.Id))
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));

            if (!string.IsNullOrWhiteSpace(user.Email))
                claims.Add(new Claim(ClaimTypes.Email, user.Email));

            // Nome de exibição (name -> full_name -> email)
            var name = TryGetMeta(user.UserMetadata, "name")
                       ?? TryGetMeta(user.UserMetadata, "full_name")
                       ?? user.Email
                       ?? "Usuário";
            claims.Add(new Claim(ClaimTypes.Name, name));

            // Avatar (picture/avatar_url) — claim custom
            var avatar = TryGetMeta(user.UserMetadata, "picture")
                         ?? TryGetMeta(user.UserMetadata, "avatar_url")
                         ?? TryGetFirstIdentityPicture(user);
            if (!string.IsNullOrWhiteSpace(avatar))
                claims.Add(new Claim("avatar_url", avatar));

            // Roles (se você gravar em app_metadata.roles)
            var rolesRaw = TryGetMeta(user.AppMetadata, "roles");
            if (!string.IsNullOrWhiteSpace(rolesRaw))
            {
                // aceita "role1,role2" ou JSON simples; aqui tratamos CSV simples
                foreach (var r in rolesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    claims.Add(new Claim(ClaimTypes.Role, r));
            }

            var identity = new ClaimsIdentity(claims, authenticationType: "Supabase");
            return new ClaimsPrincipal(identity);
        }

        private static string? TryGetMeta(IDictionary<string, object?>? dict, string key)
        {
            if (dict == null || !dict.TryGetValue(key, out var v) || v is null)
                return null;

            // Pode vir string, número ou JsonElement/Dictionary; ToString cobre os casos simples
            var s = v.ToString();
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        private static string? TryGetFirstIdentityPicture(User user)
        {
            if (user.Identities?.Count > 0)
            {
                var id0 = user.Identities[0];
                if (id0.IdentityData is Dictionary<string, object> idd &&
                    idd.TryGetValue("picture", out var v) && v is not null)
                {
                    var s = v.ToString();
                    if (!string.IsNullOrWhiteSpace(s))
                        return s;
                }
            }
            return null;
        }

        // --------- Preenche DisplayName/AvatarUrl a partir do User (para AppBar) ---------
        private void UpdateProfileFromUser(User? user)
        {
            if (user == null) { AvatarUrl = null; DisplayName = null; return; }

            // Nome
            DisplayName = TryGetMeta(user.UserMetadata, "name")
                       ?? TryGetMeta(user.UserMetadata, "full_name")
                       ?? user.Email;

            // Foto
            AvatarUrl = TryGetMeta(user.UserMetadata, "picture")
                     ?? TryGetMeta(user.UserMetadata, "avatar_url")
                     ?? TryGetFirstIdentityPicture(user);
        }
    }
}

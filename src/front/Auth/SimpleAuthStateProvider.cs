using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace HealthTech.Front.Auth
{
    /// <summary>
    /// Provider simples: começa anônimo e permite setar login/logout em runtime.
    /// </summary>
    public class SimpleAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity()); // anônimo

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_currentUser));

        public void SetUser(ClaimsPrincipal user)
        {
            _currentUser = user ?? new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void SignIn(string name, params Claim[] extraClaims)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, name ?? "Usuário")
            }.Concat(extraClaims ?? []), authenticationType: "Custom");
            SetUser(new ClaimsPrincipal(identity));
        }

        public void SignOut() => SetUser(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}

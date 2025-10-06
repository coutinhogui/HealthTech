using System.Net.Http.Headers;

namespace HealthTech.AppShell.Services;

public class BearerAuthorizationMessageHandler : DelegatingHandler
{
    private readonly SupabaseAuthService _auth;

    public BearerAuthorizationMessageHandler(SupabaseAuthService auth)
    {
        _auth = auth;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // se precisar garantir que o cliente está inicializado/refresh, você pode chamar InitializeAsync aqui
        var token = _auth.GetJwt();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}

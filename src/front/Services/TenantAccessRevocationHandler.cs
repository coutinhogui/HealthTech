using System.Net;
using System.Net.Http.Json;

namespace HealthTech.Front.Services;

public sealed class TenantAccessRevocationHandler(BffAuthService authService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Forbidden)
        {
            return response;
        }

        BffErrorResponse? error = null;
        try
        {
            error = await response.Content.ReadFromJsonAsync<BffErrorResponse>(cancellationToken);
        }
        catch
        {
            // Non-JSON errors remain available to the caller's normal handling.
        }

        if (string.Equals(
                error?.Error,
                "clinic_inactive_or_membership_revoked",
                StringComparison.OrdinalIgnoreCase))
        {
            await authService.HandleTenantAccessRevokedAsync();
        }

        return response;
    }
}

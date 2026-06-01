using System.Security.Claims;
using HealthTech.BuildingBlocks.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HealthTech.Gateway.Security;

public sealed class GatewayTenantAccessProvider(
    IOptions<TenantAccessOptions> options,
    IConfiguration configuration) : ITenantAccessProvider
{
    public async Task<IReadOnlyCollection<TenantMembership>> GetMembershipsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var subjectId = principal.FindFirstValue("sub")
                        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue("email")
                    ?? principal.FindFirstValue(ClaimTypes.Email);
        var memberships = new Dictionary<Guid, TenantMembership>();

        foreach (var membership in GetConfiguredMemberships(subjectId, email))
        {
            memberships[membership.TenantId] = membership;
        }

        var connectionString = configuration.GetConnectionString("PatientsDb");
        if (!string.IsNullOrWhiteSpace(connectionString) &&
            (!string.IsNullOrWhiteSpace(subjectId) || !string.IsNullOrWhiteSpace(email)))
        {
            await foreach (var membership in GetDatabaseMembershipsAsync(connectionString, subjectId, email, cancellationToken))
            {
                memberships[membership.TenantId] = membership;
            }
        }

        return memberships.Values.ToArray();
    }

    private IEnumerable<TenantMembership> GetConfiguredMemberships(string? subjectId, string? email)
    {
        var user = options.Value.Users.FirstOrDefault(candidate =>
            (!string.IsNullOrWhiteSpace(subjectId) &&
             string.Equals(candidate.SubjectId, subjectId, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrWhiteSpace(email) &&
             !string.IsNullOrWhiteSpace(candidate.Email) &&
             string.Equals(candidate.Email, email, StringComparison.OrdinalIgnoreCase)));

        return user?.Memberships
            .Where(membership => membership.Enabled)
            .Select(membership => new TenantMembership(
                membership.TenantId,
                string.IsNullOrWhiteSpace(membership.TenantName) ? membership.TenantId.ToString("D") : membership.TenantName,
                string.IsNullOrWhiteSpace(membership.Role) ? "member" : membership.Role)) ?? [];
    }

    private static async IAsyncEnumerable<TenantMembership> GetDatabaseMembershipsAsync(
        string connectionString,
        string? subjectId,
        string? email,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            select tenant_id, tenant_name, role
            from core.resolve_tenant_memberships(@subject_id, @email);
            """;
        command.Parameters.AddWithValue("subject_id", (object?)subjectId ?? string.Empty);
        command.Parameters.AddWithValue("email", (object?)email ?? string.Empty);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new TenantMembership(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2));
        }
    }
}

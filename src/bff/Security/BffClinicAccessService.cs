using HealthTech.BuildingBlocks.Abstractions;
using Npgsql;

namespace HealthTech.Gateway.Security;

public interface IBffClinicAccessService
{
    Task<IReadOnlyCollection<BffAccessUserResponse>> ListUsersAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<bool> UpdateUserAsync(Guid tenantId, string subjectId, BffUpdateAccessUserRequest request, CancellationToken cancellationToken);
}

public sealed class PostgresBffClinicAccessService(IConfiguration configuration) : IBffClinicAccessService
{
    private static readonly HashSet<string> AllowedRoles =
    [
        ClinicRoles.Admin,
        ClinicRoles.Professional,
        ClinicRoles.Reception,
        ClinicRoles.Billing,
        ClinicRoles.Patient
    ];

    public async Task<IReadOnlyCollection<BffAccessUserResponse>> ListUsersAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("PatientsDb")
            ?? throw new InvalidOperationException("ConnectionStrings:PatientsDb is required for clinic access management.");

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var setTenant = new NpgsqlCommand("select set_config('app.tenant_id', @tenant_id, false);", connection);
        setTenant.Parameters.AddWithValue("tenant_id", tenantId.ToString("D"));
        await setTenant.ExecuteNonQueryAsync(cancellationToken);

        await using var command = new NpgsqlCommand("""
            select subject_id,
                   email,
                   role,
                   full_name,
                   phone,
                   professional_id,
                   active
            from core.tenant_user
            where tenant_id = @tenant_id
            order by active desc, email;
            """, connection);
        command.Parameters.AddWithValue("tenant_id", tenantId);

        var users = new List<BffAccessUserResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(new BffAccessUserResponse(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetGuid(5),
                reader.GetBoolean(6)));
        }

        return users;
    }

    public async Task<bool> UpdateUserAsync(Guid tenantId, string subjectId, BffUpdateAccessUserRequest request, CancellationToken cancellationToken)
    {
        var normalizedRole = ClinicAuthorization.NormalizeRole(request.Role);
        if (!AllowedRoles.Contains(normalizedRole))
        {
            return false;
        }

        var connectionString = configuration.GetConnectionString("PatientsDb")
            ?? throw new InvalidOperationException("ConnectionStrings:PatientsDb is required for clinic access management.");

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var setTenant = new NpgsqlCommand("select set_config('app.tenant_id', @tenant_id, false);", connection);
        setTenant.Parameters.AddWithValue("tenant_id", tenantId.ToString("D"));
        await setTenant.ExecuteNonQueryAsync(cancellationToken);

        await using var command = new NpgsqlCommand("""
            update core.tenant_user
            set role = @role,
                professional_id = @professional_id,
                active = @active
            where tenant_id = @tenant_id
              and subject_id = @subject_id;
            """, connection);
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("subject_id", subjectId);
        command.Parameters.AddWithValue("role", normalizedRole);
        command.Parameters.AddWithValue("professional_id", (object?)request.ProfessionalId ?? DBNull.Value);
        command.Parameters.AddWithValue("active", request.Active);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}

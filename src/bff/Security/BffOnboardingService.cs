using Npgsql;

namespace HealthTech.Gateway.Security;

public interface IBffOnboardingService
{
    Task<BffOnboardingStatus> GetStatusAsync(
        string subjectId,
        string? email,
        BffTenantResponse? activeTenant,
        IReadOnlyCollection<BffTenantResponse> memberships,
        CancellationToken cancellationToken);

    Task<BffOnboardingCompletion> CompleteAsync(
        string subjectId,
        string? email,
        BffCompleteOnboardingRequest request,
        CancellationToken cancellationToken);
}

public sealed class PostgresBffOnboardingService(IConfiguration configuration) : IBffOnboardingService
{
    public async Task<BffOnboardingStatus> GetStatusAsync(
        string subjectId,
        string? email,
        BffTenantResponse? activeTenant,
        IReadOnlyCollection<BffTenantResponse> memberships,
        CancellationToken cancellationToken)
    {
        if (memberships.Count == 0)
        {
            return new BffOnboardingStatus(false);
        }

        if (activeTenant is null)
        {
            return new BffOnboardingStatus(true);
        }

        var connectionString = configuration.GetConnectionString("PatientsDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new BffOnboardingStatus(true);
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            select exists (
              select 1
              from core.tenant_user
              where tenant_id = @tenant_id
                and active = true
                and (subject_id = @subject_id or lower(email) = lower(@email))
                and onboarding_completed_at_utc is not null
            );
            """;
        command.Parameters.AddWithValue("tenant_id", activeTenant.TenantId);
        command.Parameters.AddWithValue("subject_id", subjectId);
        command.Parameters.AddWithValue("email", (object?)email ?? DBNull.Value);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return new BffOnboardingStatus(result is true);
    }

    public async Task<BffOnboardingCompletion> CompleteAsync(
        string subjectId,
        string? email,
        BffCompleteOnboardingRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = Guid.NewGuid();
        var tenantName = request.ClinicName.Trim();
        var responsibleName = request.ResponsibleName.Trim();
        var phone = request.Phone.Trim();
        var specialtyName = request.SpecialtyName.Trim();
        var locationName = request.LocationName.Trim();
        var timezone = string.IsNullOrWhiteSpace(request.Timezone)
            ? "America/Sao_Paulo"
            : request.Timezone.Trim();
        var userEmail = string.IsNullOrWhiteSpace(email) ? $"{subjectId}@oauth.local" : email.Trim();

        var connectionString = configuration.GetConnectionString("PatientsDb")
            ?? throw new InvalidOperationException("ConnectionStrings:PatientsDb is required for onboarding.");

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await ExecuteAsync(connection, transaction, "select set_config('app.tenant_id', @tenant_id, true);", cancellationToken, ("tenant_id", tenantId.ToString("D")));

        await ExecuteAsync(connection, transaction, """
            insert into core.tenant (id, name)
            values (@tenant_id, @tenant_name)
            on conflict (id) do update set name = excluded.name, active = true;
            """, cancellationToken, ("tenant_id", tenantId), ("tenant_name", tenantName));

        await ExecuteAsync(connection, transaction, """
            insert into core.tenant_user (
              tenant_id,
              subject_id,
              email,
              role,
              full_name,
              phone,
              onboarding_completed_at_utc
            )
            values (
              @tenant_id,
              @subject_id,
              @email,
              'admin',
              @full_name,
              @phone,
              now()
            )
            on conflict (tenant_id, subject_id) do update
            set email = excluded.email,
                role = 'admin',
                active = true,
                full_name = excluded.full_name,
                phone = excluded.phone,
                onboarding_completed_at_utc = now();
            """, cancellationToken,
            ("tenant_id", tenantId),
            ("subject_id", subjectId),
            ("email", userEmail),
            ("full_name", responsibleName),
            ("phone", phone));

        var specialtyId = Guid.NewGuid();
        await ExecuteAsync(connection, transaction, """
            insert into scheduling.specialty (id, tenant_id, name)
            values (@specialty_id, @tenant_id, @name)
            on conflict (tenant_id, name) do update set active = true;
            """, cancellationToken,
            ("specialty_id", specialtyId),
            ("tenant_id", tenantId),
            ("name", specialtyName));

        await ExecuteAsync(connection, transaction, """
            insert into scheduling.location (id, tenant_id, name, timezone)
            values (@location_id, @tenant_id, @name, @timezone)
            on conflict (tenant_id, id) do update
            set name = excluded.name,
                timezone = excluded.timezone,
                active = true;
            """, cancellationToken,
            ("location_id", Guid.NewGuid()),
            ("tenant_id", tenantId),
            ("name", locationName),
            ("timezone", timezone));

        await transaction.CommitAsync(cancellationToken);

        var membership = new BffTenantResponse(
            tenantId,
            tenantName,
            "admin",
            null,
            HealthTech.BuildingBlocks.Abstractions.ClinicAuthorization.PermissionNamesForRole("admin"));
        return new BffOnboardingCompletion(membership, [membership]);
    }

    private static async Task ExecuteAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string sql,
        CancellationToken cancellationToken,
        params (string Name, object? Value)[] parameters)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Name, parameter.Value ?? DBNull.Value);
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

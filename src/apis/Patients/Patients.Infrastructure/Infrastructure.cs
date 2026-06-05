using System.Data;
using System.Data.Common;
using Dapper;
using HealthTech.BuildingBlocks.Abstractions;
using HealthTech.Patients.Application;
using HealthTech.Patients.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthTech.Patients.Infrastructure;

public sealed class NpgsqlConnectionFactory(IConfiguration cfg) : IDbConnectionFactory
{
    public IDbConnection Create()
    {
        var cs = cfg.GetConnectionString("PatientsDb") ?? throw new InvalidOperationException("Missing PatientsDb connection string.");
        return new Npgsql.NpgsqlConnection(cs);
    }
}

public sealed class UnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);
}

public sealed class PatientRepository(IDbConnectionFactory factory) : IPatientRepository
{
    public async Task AddAsync(Patient entity, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO patients.patient (id, tenant_id, full_name, document, birth_date)
            VALUES (@Id, @TenantId, @FullName, @Document, @BirthDate);
            """;

        using var connection = factory.Create();
        try
        {
            await EnsureOpenAsync(connection, ct);
            await SetTenantAsync(connection, entity.TenantId, ct);
            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id = entity.Id.Value,
                entity.TenantId,
                entity.FullName,
                entity.Document,
                BirthDate = entity.BirthDate.ToDateTime(TimeOnly.MinValue)
            }, cancellationToken: ct));
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new DuplicatePatientDocumentException(entity.TenantId, entity.Document);
        }
    }

    public async Task<Patient?> GetAsync(Guid tenantId, Guid id, CancellationToken ct)
    {
        const string sql = """
            SELECT id AS Id, tenant_id AS TenantId, full_name AS FullName, document AS Document, birth_date AS BirthDate
            FROM patients.patient
            WHERE tenant_id = @TenantId AND id = @Id;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, ct);
        await SetTenantAsync(connection, tenantId, ct);
        var row = await connection.QuerySingleOrDefaultAsync<PatientRow>(
            new CommandDefinition(sql, new { TenantId = tenantId, Id = id }, cancellationToken: ct));

        return row is null ? null : Map(row);
    }

    public async Task<IReadOnlyCollection<Patient>> ListAsync(Guid tenantId, int skip, int take, CancellationToken ct)
    {
        const string sql = """
            SELECT id AS Id, tenant_id AS TenantId, full_name AS FullName, document AS Document, birth_date AS BirthDate
            FROM patients.patient
            WHERE tenant_id = @TenantId
            ORDER BY full_name
            OFFSET @Skip LIMIT @Take;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, ct);
        await SetTenantAsync(connection, tenantId, ct);
        var rows = await connection.QueryAsync<PatientRow>(
            new CommandDefinition(sql, new { TenantId = tenantId, Skip = Math.Max(0, skip), Take = Math.Clamp(take, 1, 200) }, cancellationToken: ct));

        return rows.Select(Map).ToArray();
    }

    public async Task<IReadOnlyCollection<Patient>> ListForProfessionalAsync(Guid tenantId, Guid professionalId, int skip, int take, CancellationToken ct)
    {
        const string sql = """
            SELECT DISTINCT p.id AS Id,
                   p.tenant_id AS TenantId,
                   p.full_name AS FullName,
                   p.document AS Document,
                   p.birth_date AS BirthDate
            FROM patients.patient p
            INNER JOIN appointments.appointment a
                ON a.tenant_id = p.tenant_id AND a.patient_id = p.id
            WHERE p.tenant_id = @TenantId
              AND a.professional_id = @ProfessionalId
            ORDER BY p.full_name
            OFFSET @Skip LIMIT @Take;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, ct);
        await SetTenantAsync(connection, tenantId, ct);
        var rows = await connection.QueryAsync<PatientRow>(
            new CommandDefinition(sql, new
            {
                TenantId = tenantId,
                ProfessionalId = professionalId,
                Skip = Math.Max(0, skip),
                Take = Math.Clamp(take, 1, 200)
            }, cancellationToken: ct));

        return rows.Select(Map).ToArray();
    }

    public async Task<bool> PatientHasAppointmentWithProfessionalAsync(Guid tenantId, Guid patientId, Guid professionalId, CancellationToken ct)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM appointments.appointment
                WHERE tenant_id = @TenantId
                  AND patient_id = @PatientId
                  AND professional_id = @ProfessionalId
            );
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, ct);
        await SetTenantAsync(connection, tenantId, ct);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new
            {
                TenantId = tenantId,
                PatientId = patientId,
                ProfessionalId = professionalId
            }, cancellationToken: ct));
    }

    public async Task<Guid?> GetPatientIdForSubjectAsync(Guid tenantId, string subjectId, CancellationToken ct)
    {
        const string sql = """
            SELECT patient_id
            FROM patients.patient_identity
            WHERE tenant_id = @TenantId
              AND subject_id = @SubjectId
              AND active
            ORDER BY linked_at_utc DESC
            LIMIT 1;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, ct);
        await SetTenantAsync(connection, tenantId, ct);
        return await connection.ExecuteScalarAsync<Guid?>(
            new CommandDefinition(sql, new { TenantId = tenantId, SubjectId = subjectId }, cancellationToken: ct));
    }

    public async Task<Guid> AddInsurancePlanAsync(Guid tenantId, string payerName, string planName, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO patients.insurance_plan (id, tenant_id, payer_name, plan_name)
            VALUES (@Id, @TenantId, @PayerName, @PlanName)
            RETURNING id;
            """;

        using var connection = factory.Create();
        try
        {
            await EnsureOpenAsync(connection, ct);
            await SetTenantAsync(connection, tenantId, ct);
            var id = Guid.NewGuid();
            return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, new
            {
                Id = id,
                TenantId = tenantId,
                PayerName = payerName,
                PlanName = planName
            }, cancellationToken: ct));
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new DuplicateInsurancePlanException(tenantId, payerName, planName);
        }
    }

    public async Task<IReadOnlyCollection<InsurancePlanReadModel>> ListInsurancePlansAsync(Guid tenantId, CancellationToken ct)
    {
        const string sql = """
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   payer_name AS PayerName,
                   plan_name AS PlanName,
                   active AS Active
            FROM patients.insurance_plan
            WHERE tenant_id = @TenantId
              AND active
            ORDER BY payer_name, plan_name;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, ct);
        await SetTenantAsync(connection, tenantId, ct);
        var rows = await connection.QueryAsync<InsurancePlanReadModel>(
            new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: ct));

        return rows.ToArray();
    }

    public async Task<bool> PlanExistsAsync(Guid tenantId, Guid planId, CancellationToken ct)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM patients.insurance_plan
                WHERE tenant_id = @TenantId
                  AND id = @PlanId
                  AND active
            );
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, ct);
        await SetTenantAsync(connection, tenantId, ct);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { TenantId = tenantId, PlanId = planId }, cancellationToken: ct));
    }

    public async Task UpsertPatientPlanAsync(Guid tenantId, Guid patientId, Guid planId, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO patients.patient_plan (tenant_id, patient_id, plan_id, active)
            VALUES (@TenantId, @PatientId, @PlanId, true)
            ON CONFLICT (tenant_id, patient_id, plan_id)
            DO UPDATE SET active = true, linked_at_utc = now();
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, ct);
        await SetTenantAsync(connection, tenantId, ct);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { TenantId = tenantId, PatientId = patientId, PlanId = planId }, cancellationToken: ct));
    }

    public async Task<IReadOnlyCollection<PatientPlanReadModel>> ListPatientPlansAsync(Guid tenantId, Guid patientId, CancellationToken ct)
    {
        const string sql = """
            SELECT pp.patient_id AS PatientId,
                   pp.plan_id AS PlanId,
                   ip.payer_name AS PayerName,
                   ip.plan_name AS PlanName,
                   pp.active AS Active,
                   pp.linked_at_utc AS LinkedAtUtc
            FROM patients.patient_plan pp
            INNER JOIN patients.insurance_plan ip
                ON ip.tenant_id = pp.tenant_id AND ip.id = pp.plan_id
            WHERE pp.tenant_id = @TenantId
              AND pp.patient_id = @PatientId
              AND pp.active
            ORDER BY pp.linked_at_utc DESC;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, ct);
        await SetTenantAsync(connection, tenantId, ct);
        var rows = await connection.QueryAsync<PatientPlanRow>(
            new CommandDefinition(sql, new { TenantId = tenantId, PatientId = patientId }, cancellationToken: ct));

        return rows.Select(row => new PatientPlanReadModel(
            row.PatientId,
            row.PlanId,
            row.PayerName,
            row.PlanName,
            row.Active,
            AsUtcOffset(row.LinkedAtUtc))).ToArray();
    }

    public async Task<Guid> AddPatientEvolutionAsync(
        Guid tenantId,
        Guid patientId,
        DateTimeOffset encounteredAtUtc,
        string subjective,
        string objective,
        string assessment,
        string plan,
        string? createdBySubject,
        CancellationToken ct)
    {
        const string sql = """
            INSERT INTO patients.patient_evolution (
                id,
                tenant_id,
                patient_id,
                encountered_at_utc,
                subjective,
                objective,
                assessment,
                plan,
                created_by_subject
            )
            VALUES (
                @Id,
                @TenantId,
                @PatientId,
                @EncounteredAtUtc,
                @Subjective,
                @Objective,
                @Assessment,
                @Plan,
                @CreatedBySubject
            )
            RETURNING id;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, ct);
        await SetTenantAsync(connection, tenantId, ct);
        var id = Guid.NewGuid();
        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, new
        {
            Id = id,
            TenantId = tenantId,
            PatientId = patientId,
            EncounteredAtUtc = encounteredAtUtc.UtcDateTime,
            Subjective = subjective,
            Objective = objective,
            Assessment = assessment,
            Plan = plan,
            CreatedBySubject = createdBySubject
        }, cancellationToken: ct));
    }

    public async Task<IReadOnlyCollection<PatientEvolutionReadModel>> ListPatientEvolutionsAsync(Guid tenantId, Guid patientId, int skip, int take, CancellationToken ct)
    {
        const string sql = """
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   patient_id AS PatientId,
                   encountered_at_utc AS EncounteredAtUtc,
                   subjective AS Subjective,
                   objective AS Objective,
                   assessment AS Assessment,
                   plan AS Plan,
                   created_by_subject AS CreatedBySubject,
                   created_at_utc AS CreatedAtUtc
            FROM patients.patient_evolution
            WHERE tenant_id = @TenantId
              AND patient_id = @PatientId
            ORDER BY encountered_at_utc DESC, created_at_utc DESC
            OFFSET @Skip LIMIT @Take;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, ct);
        await SetTenantAsync(connection, tenantId, ct);
        var rows = await connection.QueryAsync<PatientEvolutionRow>(
            new CommandDefinition(sql, new
            {
                TenantId = tenantId,
                PatientId = patientId,
                Skip = Math.Max(0, skip),
                Take = Math.Clamp(take, 1, 200)
            }, cancellationToken: ct));

        return rows.Select(row => new PatientEvolutionReadModel(
            row.Id,
            row.TenantId,
            row.PatientId,
            AsUtcOffset(row.EncounteredAtUtc),
            row.Subjective,
            row.Objective,
            row.Assessment,
            row.Plan,
            row.CreatedBySubject,
            AsUtcOffset(row.CreatedAtUtc))).ToArray();
    }

    private static Patient Map(PatientRow row)
        => new(new PatientId(row.Id), row.TenantId, row.FullName, row.Document, DateOnly.FromDateTime(row.BirthDate));

    private static DateTimeOffset AsUtcOffset(DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static Task SetTenantAsync(IDbConnection connection, Guid tenantId, CancellationToken ct)
        => connection.ExecuteAsync(new CommandDefinition(
            "select set_config('app.tenant_id', @TenantId, false);",
            new { TenantId = tenantId.ToString("D") },
            cancellationToken: ct));

    private static async Task EnsureOpenAsync(IDbConnection connection, CancellationToken ct)
    {
        if (connection.State == ConnectionState.Open)
        {
            return;
        }

        if (connection is DbConnection dbConnection)
        {
            await dbConnection.OpenAsync(ct);
            return;
        }

        connection.Open();
    }

    private sealed record PatientRow(Guid Id, Guid TenantId, string FullName, string Document, DateTime BirthDate);
    private sealed record PatientPlanRow(Guid PatientId, Guid PlanId, string PayerName, string PlanName, bool Active, DateTime LinkedAtUtc);
    private sealed record PatientEvolutionRow(
        Guid Id,
        Guid TenantId,
        Guid PatientId,
        DateTime EncounteredAtUtc,
        string Subjective,
        string Objective,
        string Assessment,
        string Plan,
        string? CreatedBySubject,
        DateTime CreatedAtUtc);
}

public static class PatientsInfraRegistration
{
    public static IServiceCollection AddPatientsInfra(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        return services;
    }
}

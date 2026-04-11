using System.Data;
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
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            Id = entity.Id.Value,
            entity.TenantId,
            entity.FullName,
            entity.Document,
            BirthDate = entity.BirthDate
        }, cancellationToken: ct));
    }

    public async Task<Patient?> GetAsync(Guid tenantId, Guid id, CancellationToken ct)
    {
        const string sql = """
            SELECT id AS Id, tenant_id AS TenantId, full_name AS FullName, document AS Document, birth_date AS BirthDate
            FROM patients.patient
            WHERE tenant_id = @TenantId AND id = @Id;
            """;

        using var connection = factory.Create();
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
        var rows = await connection.QueryAsync<PatientRow>(
            new CommandDefinition(sql, new { TenantId = tenantId, Skip = Math.Max(0, skip), Take = Math.Clamp(take, 1, 200) }, cancellationToken: ct));

        return rows.Select(Map).ToArray();
    }

    private static Patient Map(PatientRow row)
        => new(new PatientId(row.Id), row.TenantId, row.FullName, row.Document, row.BirthDate);

    private sealed record PatientRow(Guid Id, Guid TenantId, string FullName, string Document, DateOnly BirthDate);
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

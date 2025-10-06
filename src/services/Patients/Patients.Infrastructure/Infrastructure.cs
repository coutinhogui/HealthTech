namespace HealthTech.Patients.Infrastructure;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HealthTech.BuildingBlocks.Abstractions;
using HealthTech.Patients.Application;
using HealthTech.Patients.Domain;

public sealed class NpgsqlConnectionFactory(IConfiguration cfg) : IDbConnectionFactory
{
    public IDbConnection Create()
    {
        var cs = cfg.GetConnectionString("PatientsDb") ?? throw new InvalidOperationException("Missing PatientsDb connection string");
        return new Npgsql.NpgsqlConnection(cs);
    }
}

public sealed class UnitOfWork(IDbConnectionFactory f) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0); // Dapper: transacione por operação conforme necessidade.
}

public sealed class PatientRepository(IDbConnectionFactory f) : IPatientRepository
{
    public async Task AddAsync(Patient entity, CancellationToken ct)
    {
        const string sql = "INSERT INTO patients.patient(id, full_name, document, birth_date) VALUES (@Id, @FullName, @Document, @BirthDate);";
        using var con = f.Create();
        await con.ExecuteAsync(new CommandDefinition(sql, new { Id = entity.Id.Value, entity.FullName, entity.Document, BirthDate = entity.BirthDate }, cancellationToken: ct));
    }

    public async Task<Patient?> GetAsync(Guid id, CancellationToken ct)
    {
        const string sql = "SELECT id, full_name, document, birth_date FROM patients.patient WHERE id = @Id";
        using var con = f.Create();
        var row = await con.QuerySingleOrDefaultAsync(sql, new { Id = id });
        if (row is null) return null;
        return new Patient(new PatientId((Guid)row.id), (string)row.full_name, (string)row.document, DateOnly.FromDateTime((DateTime)row.birth_date));
    }

    public async Task<IEnumerable<Patient>> ListAsync(int skip, int take, CancellationToken ct)
    {
        const string sql = "SELECT id, full_name, document, birth_date FROM patients.patient ORDER BY full_name OFFSET @Skip LIMIT @Take";
        using var con = f.Create();
        var rows = await con.QueryAsync(sql, new { Skip = skip, Take = take });
        return rows.Select(r => new Patient(new PatientId((Guid)r.id), (string)r.full_name, (string)r.document, DateOnly.FromDateTime((DateTime)r.birth_date)));
    }
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

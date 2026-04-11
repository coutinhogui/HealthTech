using System.Data;
using Dapper;
using HealthTech.Appointments.Application;
using HealthTech.Appointments.Domain;
using HealthTech.BuildingBlocks.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthTech.Appointments.Infrastructure;

public sealed class NpgsqlConnectionFactory(IConfiguration cfg) : IDbConnectionFactory
{
    public IDbConnection Create()
    {
        var cs = cfg.GetConnectionString("AppointmentsDb")
                 ?? cfg.GetConnectionString("PatientsDb")
                 ?? throw new InvalidOperationException("Missing AppointmentsDb connection string.");
        return new Npgsql.NpgsqlConnection(cs);
    }
}

public sealed class UnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);
}

public sealed class AppointmentRepository(IDbConnectionFactory factory) : IAppointmentRepository
{
    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO appointments.appointment (
                id, tenant_id, patient_id, professional_id, starts_at_utc, ends_at_utc, status, notes)
            VALUES (
                @Id, @TenantId, @PatientId, @ProfessionalId, @StartsAtUtc, @EndsAtUtc, @Status, @Notes);
            """;

        using var connection = factory.Create();
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            Id = appointment.Id.Value,
            appointment.TenantId,
            appointment.PatientId,
            appointment.ProfessionalId,
            appointment.StartsAtUtc,
            appointment.EndsAtUtc,
            appointment.Status,
            appointment.Notes
        }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<Appointment>> ListAsync(
        Guid tenantId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   patient_id AS PatientId,
                   professional_id AS ProfessionalId,
                   starts_at_utc AS StartsAtUtc,
                   ends_at_utc AS EndsAtUtc,
                   status AS Status,
                   notes AS Notes
            FROM appointments.appointment
            WHERE tenant_id = @TenantId
              AND starts_at_utc >= @FromUtc
              AND starts_at_utc < @ToUtc
            ORDER BY starts_at_utc;
            """;

        using var connection = factory.Create();
        var rows = await connection.QueryAsync<AppointmentRow>(
            new CommandDefinition(sql, new { TenantId = tenantId, FromUtc = fromUtc.UtcDateTime, ToUtc = toUtc.UtcDateTime }, cancellationToken: cancellationToken));

        return rows.Select(Map).ToArray();
    }

    private static Appointment Map(AppointmentRow row)
        => new(
            new AppointmentId(row.Id),
            row.TenantId,
            row.PatientId,
            row.ProfessionalId,
            row.StartsAtUtc,
            row.EndsAtUtc,
            row.Status,
            row.Notes);

    private sealed record AppointmentRow(
        Guid Id,
        Guid TenantId,
        Guid PatientId,
        Guid ProfessionalId,
        DateTimeOffset StartsAtUtc,
        DateTimeOffset EndsAtUtc,
        string Status,
        string? Notes);
}

public static class AppointmentsInfrastructureRegistration
{
    public static IServiceCollection AddAppointmentsInfra(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        return services;
    }
}

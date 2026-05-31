using System.Data;
using System.Data.Common;
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
                id, tenant_id, patient_id, professional_id, location_id, starts_at_utc, ends_at_utc, status, notes)
            VALUES (
                @Id, @TenantId, @PatientId, @ProfessionalId, @LocationId, @StartsAtUtc, @EndsAtUtc, @Status, @Notes);
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, appointment.TenantId, cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            Id = appointment.Id.Value,
            appointment.TenantId,
            appointment.PatientId,
            appointment.ProfessionalId,
            appointment.LocationId,
            appointment.StartsAtUtc,
            appointment.EndsAtUtc,
            appointment.Status,
            appointment.Notes
        }, cancellationToken: cancellationToken));
    }

    public async Task<bool> PatientExistsAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM patients.patient
                WHERE tenant_id = @TenantId AND id = @PatientId
            );
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { TenantId = tenantId, PatientId = patientId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> ProfessionalExistsAsync(Guid tenantId, Guid professionalId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM scheduling.professional
                WHERE tenant_id = @TenantId AND id = @ProfessionalId AND active
            );
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { TenantId = tenantId, ProfessionalId = professionalId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> HasProfessionalConflictAsync(
        Guid tenantId,
        Guid professionalId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        Guid? excludingAppointmentId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM appointments.appointment
                WHERE tenant_id = @TenantId
                  AND professional_id = @ProfessionalId
                  AND status IN ('scheduled', 'confirmed')
                  AND starts_at_utc < @EndsAtUtc
                  AND ends_at_utc > @StartsAtUtc
                  AND (@ExcludingAppointmentId IS NULL OR id <> @ExcludingAppointmentId)
            );
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new
            {
                TenantId = tenantId,
                ProfessionalId = professionalId,
                StartsAtUtc = startsAtUtc.UtcDateTime,
                EndsAtUtc = endsAtUtc.UtcDateTime,
                ExcludingAppointmentId = excludingAppointmentId
            }, cancellationToken: cancellationToken));
    }

    public async Task<bool> LocationExistsAsync(Guid tenantId, Guid locationId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM scheduling.location
                WHERE tenant_id = @TenantId AND id = @LocationId AND active
            );
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { TenantId = tenantId, LocationId = locationId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> CancelAsync(Guid tenantId, Guid appointmentId, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE appointments.appointment
            SET status = 'cancelled'
            WHERE tenant_id = @TenantId
              AND id = @AppointmentId
              AND status IN ('scheduled', 'confirmed');
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { TenantId = tenantId, AppointmentId = appointmentId }, cancellationToken: cancellationToken));
        return affected > 0;
    }

    public async Task<bool> RescheduleAsync(
        Guid tenantId,
        Guid appointmentId,
        Guid professionalId,
        Guid? locationId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        string? notes,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE appointments.appointment
            SET professional_id = @ProfessionalId,
                location_id = @LocationId,
                starts_at_utc = @StartsAtUtc,
                ends_at_utc = @EndsAtUtc,
                notes = @Notes
            WHERE tenant_id = @TenantId
              AND id = @AppointmentId
              AND status IN ('scheduled', 'confirmed');
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                TenantId = tenantId,
                AppointmentId = appointmentId,
                ProfessionalId = professionalId,
                LocationId = locationId,
                StartsAtUtc = startsAtUtc.UtcDateTime,
                EndsAtUtc = endsAtUtc.UtcDateTime,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
            }, cancellationToken: cancellationToken));

        return affected > 0;
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
                   location_id AS LocationId,
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
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var rows = await connection.QueryAsync<AppointmentRow>(
            new CommandDefinition(sql, new { TenantId = tenantId, FromUtc = fromUtc.UtcDateTime, ToUtc = toUtc.UtcDateTime }, cancellationToken: cancellationToken));

        return rows.Select(Map).ToArray();
    }

    public async Task<IReadOnlyCollection<BusyWindowReadModel>> ListBusyWindowsAsync(
        Guid tenantId,
        Guid professionalId,
        Guid? locationId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT starts_at_utc AS StartsAtUtc,
                   ends_at_utc AS EndsAtUtc
            FROM appointments.appointment
            WHERE tenant_id = @TenantId
              AND status IN ('scheduled', 'confirmed')
              AND starts_at_utc < @ToUtc
              AND ends_at_utc > @FromUtc
              AND (
                    professional_id = @ProfessionalId
                    OR (@LocationId IS NOT NULL AND location_id = @LocationId)
                  )
            ORDER BY starts_at_utc;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var rows = await connection.QueryAsync<BusyWindowRow>(
            new CommandDefinition(sql, new
            {
                TenantId = tenantId,
                ProfessionalId = professionalId,
                LocationId = locationId,
                FromUtc = fromUtc.UtcDateTime,
                ToUtc = toUtc.UtcDateTime
            }, cancellationToken: cancellationToken));

        return rows.Select(static row => new BusyWindowReadModel(
            AsUtcOffset(row.StartsAtUtc),
            AsUtcOffset(row.EndsAtUtc))).ToArray();
    }

    public async Task<IReadOnlyCollection<SpecialtyReadModel>> ListSpecialtiesAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS Id,
                   name AS Name,
                   active AS Active
            FROM scheduling.specialty
            WHERE tenant_id = @TenantId
              AND active
            ORDER BY name;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var rows = await connection.QueryAsync<SpecialtyReadModel>(
            new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: cancellationToken));

        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<LocationReadModel>> ListLocationsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS Id,
                   name AS Name,
                   timezone AS Timezone,
                   active AS Active
            FROM scheduling.location
            WHERE tenant_id = @TenantId
              AND active
            ORDER BY name;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var rows = await connection.QueryAsync<LocationReadModel>(
            new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: cancellationToken));

        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<ProfessionalReadModel>> ListProfessionalsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT p.id AS Id,
                   p.full_name AS FullName,
                   p.specialty_id AS SpecialtyId,
                   s.name AS SpecialtyName,
                   p.active AS Active
            FROM scheduling.professional p
            INNER JOIN scheduling.specialty s
                ON s.tenant_id = p.tenant_id AND s.id = p.specialty_id
            WHERE p.tenant_id = @TenantId
              AND p.active
            ORDER BY p.full_name;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var rows = await connection.QueryAsync<ProfessionalOptionRow>(
            new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: cancellationToken));

        return rows
            .Select(row => new ProfessionalReadModel(row.Id, row.FullName, row.SpecialtyId, row.SpecialtyName, row.Active))
            .ToArray();
    }

    public async Task<bool> SpecialtyExistsAsync(Guid tenantId, Guid specialtyId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM scheduling.specialty
                WHERE tenant_id = @TenantId AND id = @SpecialtyId AND active
            );
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { TenantId = tenantId, SpecialtyId = specialtyId }, cancellationToken: cancellationToken));
    }

    public async Task<Guid> AddSpecialtyAsync(Guid tenantId, string name, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO scheduling.specialty (id, tenant_id, name)
            VALUES (@Id, @TenantId, @Name)
            RETURNING id;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var id = Guid.NewGuid();
        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(sql, new { Id = id, TenantId = tenantId, Name = name }, cancellationToken: cancellationToken));
    }

    public async Task<Guid> AddLocationAsync(Guid tenantId, string name, string timezone, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO scheduling.location (id, tenant_id, name, timezone)
            VALUES (@Id, @TenantId, @Name, @Timezone)
            RETURNING id;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var id = Guid.NewGuid();
        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(sql, new { Id = id, TenantId = tenantId, Name = name, Timezone = timezone }, cancellationToken: cancellationToken));
    }

    public async Task<Guid> AddProfessionalAsync(Guid tenantId, string fullName, Guid specialtyId, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO scheduling.professional (id, tenant_id, full_name, specialty_id)
            VALUES (@Id, @TenantId, @FullName, @SpecialtyId)
            RETURNING id;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var id = Guid.NewGuid();
        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(sql, new { Id = id, TenantId = tenantId, FullName = fullName, SpecialtyId = specialtyId }, cancellationToken: cancellationToken));
    }

    public async Task<Guid> AddChargeAsync(
        Guid tenantId,
        Guid appointmentId,
        Guid patientId,
        string payerType,
        string description,
        decimal amount,
        DateOnly dueDate,
        string? createdBySubject,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO appointments.charge (
                id,
                tenant_id,
                appointment_id,
                patient_id,
                payer_type,
                description,
                amount,
                due_date,
                status,
                created_by_subject
            )
            VALUES (
                @Id,
                @TenantId,
                @AppointmentId,
                @PatientId,
                @PayerType,
                @Description,
                @Amount,
                @DueDate,
                'open',
                @CreatedBySubject
            )
            RETURNING id;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var id = Guid.NewGuid();
        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, new
        {
            Id = id,
            TenantId = tenantId,
            AppointmentId = appointmentId,
            PatientId = patientId,
            PayerType = payerType,
            Description = description,
            Amount = amount,
            DueDate = dueDate.ToDateTime(TimeOnly.MinValue),
            CreatedBySubject = createdBySubject
        }, cancellationToken: cancellationToken));
    }

    public async Task<bool> MarkChargeAsPaidAsync(Guid tenantId, Guid chargeId, DateTimeOffset paidAtUtc, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE appointments.charge
            SET status = 'paid',
                paid_at_utc = @PaidAtUtc
            WHERE tenant_id = @TenantId
              AND id = @ChargeId;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            TenantId = tenantId,
            ChargeId = chargeId,
            PaidAtUtc = paidAtUtc.UtcDateTime
        }, cancellationToken: cancellationToken));
        return affected > 0;
    }

    public async Task<IReadOnlyCollection<ChargeReadModel>> ListChargesAsync(Guid tenantId, string? status, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   appointment_id AS AppointmentId,
                   patient_id AS PatientId,
                   payer_type AS PayerType,
                   description AS Description,
                   amount AS Amount,
                   due_date AS DueDate,
                   status AS Status,
                   paid_at_utc AS PaidAtUtc,
                   batch_id AS BatchId,
                   created_at_utc AS CreatedAtUtc
            FROM appointments.charge
            WHERE tenant_id = @TenantId
              AND (@Status IS NULL OR status = @Status)
            ORDER BY due_date DESC, created_at_utc DESC;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var rows = await connection.QueryAsync<ChargeRow>(new CommandDefinition(sql, new
        {
            TenantId = tenantId,
            Status = status
        }, cancellationToken: cancellationToken));

        return rows.Select(row => new ChargeReadModel(
            row.Id,
            row.TenantId,
            row.AppointmentId,
            row.PatientId,
            row.PayerType,
            row.Description,
            row.Amount,
            DateOnly.FromDateTime(row.DueDate),
            row.Status,
            row.PaidAtUtc is null ? null : AsUtcOffset(row.PaidAtUtc.Value),
            row.BatchId,
            AsUtcOffset(row.CreatedAtUtc))).ToArray();
    }

    public async Task<Guid> CloseBatchAsync(Guid tenantId, DateOnly competenceMonth, string? createdBySubject, CancellationToken cancellationToken)
    {
        const string insertBatchSql = """
            INSERT INTO appointments.billing_batch (
                id,
                tenant_id,
                competence_month,
                total_amount,
                status,
                created_by_subject
            )
            VALUES (
                @BatchId,
                @TenantId,
                @CompetenceMonth,
                (
                    SELECT COALESCE(SUM(amount), 0)
                    FROM appointments.charge
                    WHERE tenant_id = @TenantId
                      AND batch_id IS NULL
                      AND date_trunc('month', due_date)::date = @CompetenceMonth
                      AND status IN ('open', 'overdue')
                ),
                'closed',
                @CreatedBySubject
            )
            ON CONFLICT (tenant_id, competence_month)
            DO UPDATE SET
                total_amount = EXCLUDED.total_amount,
                created_by_subject = EXCLUDED.created_by_subject
            RETURNING id;
            """;

        const string attachChargesSql = """
            UPDATE appointments.charge
            SET batch_id = @BatchId,
                status = 'batched'
            WHERE tenant_id = @TenantId
              AND batch_id IS NULL
              AND date_trunc('month', due_date)::date = @CompetenceMonth
              AND status IN ('open', 'overdue');
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var batchId = Guid.NewGuid();
        var monthStart = new DateOnly(competenceMonth.Year, competenceMonth.Month, 1).ToDateTime(TimeOnly.MinValue);
        var persistedBatchId = await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(insertBatchSql, new
        {
            BatchId = batchId,
            TenantId = tenantId,
            CompetenceMonth = monthStart,
            CreatedBySubject = createdBySubject
        }, cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(attachChargesSql, new
        {
            BatchId = persistedBatchId,
            TenantId = tenantId,
            CompetenceMonth = monthStart
        }, cancellationToken: cancellationToken));

        return persistedBatchId;
    }

    public async Task<IReadOnlyCollection<BillingBatchReadModel>> ListBatchesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   competence_month AS CompetenceMonth,
                   total_amount AS TotalAmount,
                   status AS Status,
                   created_at_utc AS CreatedAtUtc
            FROM appointments.billing_batch
            WHERE tenant_id = @TenantId
            ORDER BY competence_month DESC, created_at_utc DESC;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var rows = await connection.QueryAsync<BillingBatchRow>(
            new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: cancellationToken));

        return rows.Select(row => new BillingBatchReadModel(
            row.Id,
            row.TenantId,
            DateOnly.FromDateTime(row.CompetenceMonth),
            row.TotalAmount,
            row.Status,
            AsUtcOffset(row.CreatedAtUtc))).ToArray();
    }

    public async Task<bool> AppointmentExistsAsync(Guid tenantId, Guid appointmentId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM appointments.appointment
                WHERE tenant_id = @TenantId
                  AND id = @AppointmentId
            );
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { TenantId = tenantId, AppointmentId = appointmentId }, cancellationToken: cancellationToken));
    }

    public async Task<Guid> AddManualWhatsappIntentAsync(
        Guid tenantId,
        Guid appointmentId,
        string recipientPhone,
        string messageText,
        string? createdBySubject,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO appointments.whatsapp_manual_message (
                id,
                tenant_id,
                appointment_id,
                recipient_phone,
                message_text,
                status,
                provider,
                provider_message_id,
                created_by_subject,
                sent_at_utc
            )
            VALUES (
                @Id,
                @TenantId,
                @AppointmentId,
                @RecipientPhone,
                @MessageText,
                'intent',
                NULL,
                NULL,
                @CreatedBySubject,
                NULL
            )
            RETURNING id;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var id = Guid.NewGuid();
        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(sql, new
            {
                Id = id,
                TenantId = tenantId,
                AppointmentId = appointmentId,
                RecipientPhone = recipientPhone,
                MessageText = messageText,
                CreatedBySubject = createdBySubject
            }, cancellationToken: cancellationToken));
    }

    public async Task<bool> MarkManualWhatsappSentAsync(
        Guid tenantId,
        Guid appointmentId,
        Guid messageId,
        string? provider,
        string? providerMessageId,
        DateTimeOffset sentAtUtc,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE appointments.whatsapp_manual_message
            SET status = 'sent',
                provider = @Provider,
                provider_message_id = @ProviderMessageId,
                sent_at_utc = @SentAtUtc
            WHERE tenant_id = @TenantId
              AND appointment_id = @AppointmentId
              AND id = @MessageId;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                TenantId = tenantId,
                AppointmentId = appointmentId,
                MessageId = messageId,
                Provider = provider,
                ProviderMessageId = providerMessageId,
                SentAtUtc = sentAtUtc.UtcDateTime
            }, cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<IReadOnlyCollection<ManualWhatsappMessageReadModel>> ListManualWhatsappMessagesAsync(
        Guid tenantId,
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS Id,
                   appointment_id AS AppointmentId,
                   recipient_phone AS RecipientPhone,
                   message_text AS MessageText,
                   status AS Status,
                   provider AS Provider,
                   provider_message_id AS ProviderMessageId,
                   created_by_subject AS CreatedBySubject,
                   created_at_utc AS CreatedAtUtc,
                   sent_at_utc AS SentAtUtc
            FROM appointments.whatsapp_manual_message
            WHERE tenant_id = @TenantId
              AND appointment_id = @AppointmentId
            ORDER BY created_at_utc DESC;
            """;

        using var connection = factory.Create();
        await EnsureOpenAsync(connection, cancellationToken);
        await SetTenantAsync(connection, tenantId, cancellationToken);
        var rows = await connection.QueryAsync<ManualWhatsappMessageRow>(
            new CommandDefinition(sql, new { TenantId = tenantId, AppointmentId = appointmentId }, cancellationToken: cancellationToken));

        return rows.Select(row => new ManualWhatsappMessageReadModel(
            row.Id,
            row.AppointmentId,
            row.RecipientPhone,
            row.MessageText,
            row.Status,
            row.Provider,
            row.ProviderMessageId,
            row.CreatedBySubject,
            AsUtcOffset(row.CreatedAtUtc),
            row.SentAtUtc is null ? null : AsUtcOffset(row.SentAtUtc.Value))).ToArray();
    }

    private static Appointment Map(AppointmentRow row)
        => new(
            new AppointmentId(row.Id),
            row.TenantId,
            row.PatientId,
            row.ProfessionalId,
            row.LocationId,
            AsUtcOffset(row.StartsAtUtc),
            AsUtcOffset(row.EndsAtUtc),
            row.Status,
            row.Notes);

    private static DateTimeOffset AsUtcOffset(DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static Task SetTenantAsync(IDbConnection connection, Guid tenantId, CancellationToken cancellationToken)
        => connection.ExecuteAsync(new CommandDefinition(
            "select set_config('app.tenant_id', @TenantId, false);",
            new { TenantId = tenantId.ToString("D") },
            cancellationToken: cancellationToken));

    private static async Task EnsureOpenAsync(IDbConnection connection, CancellationToken cancellationToken)
    {
        if (connection.State == ConnectionState.Open)
        {
            return;
        }

        if (connection is DbConnection dbConnection)
        {
            await dbConnection.OpenAsync(cancellationToken);
            return;
        }

        connection.Open();
    }

    private sealed record AppointmentRow(
        Guid Id,
        Guid TenantId,
        Guid PatientId,
        Guid ProfessionalId,
        Guid? LocationId,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        string Status,
        string? Notes);

    private sealed record BusyWindowRow(DateTime StartsAtUtc, DateTime EndsAtUtc);
    private sealed record ProfessionalOptionRow(Guid Id, string FullName, Guid SpecialtyId, string SpecialtyName, bool Active);
    private sealed record ManualWhatsappMessageRow(
        Guid Id,
        Guid AppointmentId,
        string RecipientPhone,
        string MessageText,
        string Status,
        string? Provider,
        string? ProviderMessageId,
        string? CreatedBySubject,
        DateTime CreatedAtUtc,
        DateTime? SentAtUtc);
    private sealed record ChargeRow(
        Guid Id,
        Guid TenantId,
        Guid AppointmentId,
        Guid PatientId,
        string PayerType,
        string Description,
        decimal Amount,
        DateTime DueDate,
        string Status,
        DateTime? PaidAtUtc,
        Guid? BatchId,
        DateTime CreatedAtUtc);
    private sealed record BillingBatchRow(
        Guid Id,
        Guid TenantId,
        DateTime CompetenceMonth,
        decimal TotalAmount,
        string Status,
        DateTime CreatedAtUtc);
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

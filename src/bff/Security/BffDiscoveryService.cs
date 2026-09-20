using Npgsql;

namespace HealthTech.Gateway.Security;

public interface IBffDiscoveryService
{
    Task<IReadOnlyCollection<BffDiscoverySearchResponse>> SearchAsync(BffDiscoverySearchRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BffDiscoveryClinicResponse>> ListClinicsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BffDiscoverySpecialtyResponse>> ListSpecialtiesAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BffDiscoveryProfessionalResponse>> ListProfessionalsAsync(Guid tenantId, Guid? specialtyId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BffDiscoverySlotResponse>> ListSlotsAsync(
        Guid tenantId,
        Guid professionalId,
        Guid? locationId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int slotMinutes,
        CancellationToken cancellationToken);
    Task<BffDiscoveryBookingResult> BookAsync(BffDiscoveryBookingRequest request, CancellationToken cancellationToken);
}

public sealed class PostgresBffDiscoveryService(IConfiguration configuration) : IBffDiscoveryService
{
    public async Task<IReadOnlyCollection<BffDiscoverySearchResponse>> SearchAsync(
        BffDiscoverySearchRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            select tenant_id,
                   professional_id,
                   location_id,
                   professional_name,
                   specialty_name,
                   clinic_name,
                   location_name,
                   city,
                   state,
                   region_label
            from scheduling.discovery_search(@mode, @query, @clinic, @specialty, @region, @take, @latitude::numeric, @longitude::numeric);
            """, connection);
        command.Parameters.AddWithValue("mode", request.Mode);
        command.Parameters.AddWithValue("query", (object?)NormalizeOptional(request.Query) ?? DBNull.Value);
        command.Parameters.AddWithValue("clinic", (object?)NormalizeOptional(request.Clinic) ?? DBNull.Value);
        command.Parameters.AddWithValue("specialty", (object?)NormalizeOptional(request.Specialty) ?? DBNull.Value);
        command.Parameters.AddWithValue("region", (object?)NormalizeOptional(request.Region) ?? DBNull.Value);
        command.Parameters.AddWithValue("take", request.Take);
        command.Parameters.AddWithValue("latitude", (object?)request.Latitude ?? DBNull.Value);
        command.Parameters.AddWithValue("longitude", (object?)request.Longitude ?? DBNull.Value);

        var results = new List<BffDiscoverySearchResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new BffDiscoverySearchResponse(
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.IsDBNull(2) ? null : reader.GetGuid(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.IsDBNull(9) ? "Regiao nao informada" : reader.GetString(9),
                []));
        }

        if (results.Count == 0)
        {
            return results;
        }

        var from = RoundUp(DateTimeOffset.UtcNow.AddHours(1), TimeSpan.FromMinutes(30));
        var to = from.AddDays(7);
        var withSlots = new List<BffDiscoverySearchResponse>(results.Count);
        foreach (var result in results)
        {
            var slots = await ListSlotsAsync(
                result.TenantId,
                result.ProfessionalId,
                result.LocationId,
                from,
                to,
                30,
                cancellationToken);

            withSlots.Add(result with { NextSlots = slots.Take(4).ToArray() });
        }

        return withSlots;
    }

    public async Task<IReadOnlyCollection<BffDiscoveryClinicResponse>> ListClinicsAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            select id, name, active
            from core.discovery_list_clinics();
            """, connection);

        var clinics = new List<BffDiscoveryClinicResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            clinics.Add(new BffDiscoveryClinicResponse(reader.GetGuid(0), reader.GetString(1), reader.GetBoolean(2)));
        }

        return clinics;
    }

    public async Task<IReadOnlyCollection<BffDiscoverySpecialtyResponse>> ListSpecialtiesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            select id, name, active
            from scheduling.discovery_list_specialties(@tenant_id);
            """, connection);
        command.Parameters.AddWithValue("tenant_id", tenantId);

        var specialties = new List<BffDiscoverySpecialtyResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            specialties.Add(new BffDiscoverySpecialtyResponse(reader.GetGuid(0), reader.GetString(1), reader.GetBoolean(2)));
        }

        return specialties;
    }

    public async Task<IReadOnlyCollection<BffDiscoveryProfessionalResponse>> ListProfessionalsAsync(
        Guid tenantId,
        Guid? specialtyId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            select id, full_name, specialty_id, specialty_name, active
            from scheduling.discovery_list_professionals(@tenant_id, @specialty_id);
            """, connection);
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("specialty_id", (object?)specialtyId ?? DBNull.Value);

        var professionals = new List<BffDiscoveryProfessionalResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            professionals.Add(new BffDiscoveryProfessionalResponse(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetGuid(2),
                reader.GetString(3),
                reader.GetBoolean(4)));
        }

        return professionals;
    }

    public async Task<IReadOnlyCollection<BffDiscoverySlotResponse>> ListSlotsAsync(
        Guid tenantId,
        Guid professionalId,
        Guid? locationId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int slotMinutes,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            select starts_at_utc, ends_at_utc
            from appointments.discovery_list_busy_windows(@tenant_id, @professional_id, @location_id, @from_utc, @to_utc);
            """, connection);
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("professional_id", professionalId);
        command.Parameters.AddWithValue("location_id", (object?)locationId ?? DBNull.Value);
        command.Parameters.AddWithValue("from_utc", fromUtc.UtcDateTime);
        command.Parameters.AddWithValue("to_utc", toUtc.UtcDateTime);

        var busy = new List<DiscoveryBusyWindow>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            busy.Add(new DiscoveryBusyWindow(
                new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime(0), DateTimeKind.Utc)),
                new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime(1), DateTimeKind.Utc))));
        }

        return CalculateSlots(fromUtc, toUtc, TimeSpan.FromMinutes(slotMinutes), busy)
            .Select(slot => new BffDiscoverySlotResponse(professionalId, slot.StartsAtUtc, slot.EndsAtUtc))
            .ToArray();
    }

    public async Task<BffDiscoveryBookingResult> BookAsync(BffDiscoveryBookingRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            select appointments.discovery_book_appointment(
              @tenant_id,
              @professional_id,
              @location_id,
              @starts_at_utc,
              @ends_at_utc,
              @patient_name,
              @patient_document,
              @patient_birth_date::date,
              @patient_email,
              @patient_phone,
              @notes
            );
            """, connection);
        command.Parameters.AddWithValue("tenant_id", request.TenantId);
        command.Parameters.AddWithValue("professional_id", request.ProfessionalId);
        command.Parameters.AddWithValue("location_id", (object?)request.LocationId ?? DBNull.Value);
        command.Parameters.AddWithValue("starts_at_utc", request.StartsAtUtc.UtcDateTime);
        command.Parameters.AddWithValue("ends_at_utc", request.EndsAtUtc.UtcDateTime);
        command.Parameters.AddWithValue("patient_name", request.PatientName.Trim());
        command.Parameters.AddWithValue("patient_document", request.PatientDocument.Trim());
        command.Parameters.AddWithValue("patient_birth_date", request.PatientBirthDate.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("patient_email", request.PatientEmail?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("patient_phone", request.PatientPhone?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("notes", (object?)NormalizeOptional(request.Notes) ?? DBNull.Value);

        try
        {
            var id = await command.ExecuteScalarAsync(cancellationToken);
            return id is Guid appointmentId
                ? BffDiscoveryBookingResult.Ok(appointmentId)
                : BffDiscoveryBookingResult.Fail("discovery_booking_failed", "Nao foi possivel criar o agendamento.");
        }
        catch (PostgresException ex) when (ex.MessageText.Contains("appointment_conflict", StringComparison.OrdinalIgnoreCase))
        {
            return BffDiscoveryBookingResult.Fail("appointment_conflict", "Esse horario acabou de ser reservado. Escolha outro horario.");
        }
        catch (PostgresException ex) when (ex.MessageText.Contains("clinic_not_found", StringComparison.OrdinalIgnoreCase))
        {
            return BffDiscoveryBookingResult.Fail("clinic_not_found", "Clinica nao encontrada.");
        }
        catch (PostgresException ex) when (ex.MessageText.Contains("professional_not_found", StringComparison.OrdinalIgnoreCase))
        {
            return BffDiscoveryBookingResult.Fail("professional_not_found", "Profissional nao encontrado.");
        }
        catch (PostgresException ex) when (ex.MessageText.Contains("location_not_found", StringComparison.OrdinalIgnoreCase))
        {
            return BffDiscoveryBookingResult.Fail("location_not_found", "Unidade nao encontrada.");
        }
    }

    private async Task<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("PatientsDb")
            ?? throw new InvalidOperationException("ConnectionStrings:PatientsDb is required for discovery.");
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTimeOffset RoundUp(DateTimeOffset value, TimeSpan interval)
    {
        var remainder = value.Ticks % interval.Ticks;
        return remainder == 0 ? value : new DateTimeOffset(value.Ticks + interval.Ticks - remainder, value.Offset);
    }

    private static IReadOnlyCollection<DiscoveryBusyWindow> CalculateSlots(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        TimeSpan slotDuration,
        IReadOnlyCollection<DiscoveryBusyWindow> busyWindows)
    {
        if (toUtc <= fromUtc || slotDuration <= TimeSpan.Zero)
        {
            return [];
        }

        var mergedBusyWindows = MergeBusyWindows(busyWindows, fromUtc, toUtc);
        var slots = new List<DiscoveryBusyWindow>();
        var cursor = fromUtc;
        var windowIndex = 0;

        while (cursor + slotDuration <= toUtc)
        {
            var slotEnd = cursor + slotDuration;

            while (windowIndex < mergedBusyWindows.Count && mergedBusyWindows[windowIndex].EndsAtUtc <= cursor)
            {
                windowIndex++;
            }

            var overlaps = windowIndex < mergedBusyWindows.Count
                           && mergedBusyWindows[windowIndex].StartsAtUtc < slotEnd
                           && mergedBusyWindows[windowIndex].EndsAtUtc > cursor;

            if (!overlaps)
            {
                slots.Add(new DiscoveryBusyWindow(cursor, slotEnd));
            }

            cursor = slotEnd;
        }

        return slots;
    }

    private static List<DiscoveryBusyWindow> MergeBusyWindows(
        IReadOnlyCollection<DiscoveryBusyWindow> busyWindows,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc)
    {
        var ordered = busyWindows
            .Where(window => window.EndsAtUtc > fromUtc && window.StartsAtUtc < toUtc)
            .Select(window => new DiscoveryBusyWindow(
                window.StartsAtUtc < fromUtc ? fromUtc : window.StartsAtUtc,
                window.EndsAtUtc > toUtc ? toUtc : window.EndsAtUtc))
            .OrderBy(window => window.StartsAtUtc)
            .ToArray();

        if (ordered.Length == 0)
        {
            return [];
        }

        var merged = new List<DiscoveryBusyWindow> { ordered[0] };
        for (var i = 1; i < ordered.Length; i++)
        {
            var current = ordered[i];
            var previous = merged[^1];

            if (current.StartsAtUtc <= previous.EndsAtUtc)
            {
                merged[^1] = new DiscoveryBusyWindow(
                    previous.StartsAtUtc,
                    current.EndsAtUtc > previous.EndsAtUtc ? current.EndsAtUtc : previous.EndsAtUtc);
                continue;
            }

            merged.Add(current);
        }

        return merged;
    }

    private sealed record DiscoveryBusyWindow(DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc);
}

public sealed record BffDiscoveryClinicResponse(Guid Id, string Name, bool Active);
public sealed record BffDiscoverySpecialtyResponse(Guid Id, string Name, bool Active);
public sealed record BffDiscoveryProfessionalResponse(Guid Id, string FullName, Guid SpecialtyId, string SpecialtyName, bool Active);
public sealed record BffDiscoverySlotResponse(Guid ProfessionalId, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc);
public sealed record BffDiscoverySearchRequest(
    string Mode,
    string? Query,
    string? Clinic,
    string? Specialty,
    string? Region,
    int Take,
    double? Latitude = null,
    double? Longitude = null);
public sealed record BffDiscoverySearchResponse(
    Guid TenantId,
    Guid ProfessionalId,
    Guid? LocationId,
    string ProfessionalName,
    string SpecialtyName,
    string ClinicName,
    string? LocationName,
    string? City,
    string? State,
    string RegionLabel,
    IReadOnlyCollection<BffDiscoverySlotResponse> NextSlots);

public sealed record BffDiscoveryBookingRequest(
    Guid TenantId,
    Guid ProfessionalId,
    Guid? LocationId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string PatientName,
    string PatientDocument,
    DateOnly PatientBirthDate,
    string? PatientEmail,
    string? PatientPhone,
    string? Notes);

public sealed record BffDiscoveryBookingResult(bool Success, Guid? AppointmentId, string? Code, string? Message)
{
    public static BffDiscoveryBookingResult Ok(Guid appointmentId) => new(true, appointmentId, null, null);
    public static BffDiscoveryBookingResult Fail(string code, string message) => new(false, null, code, message);
}

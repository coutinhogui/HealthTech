using System.Collections.Concurrent;
using System.Security.Claims;
using HealthTech.BuildingBlocks.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HealthTech.Gateway.Security;

public sealed class SystemAdminOptions
{
    public const string SectionName = "SystemAdmin";
    public List<SystemAdminUserOptions> Users { get; init; } = [];
}

public sealed class SystemAdminUserOptions
{
    public string SubjectId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool Active { get; init; } = true;
}

public interface IBffSaaSAdminService
{
    Task<bool> IsSystemAdminAsync(string? subjectId, string? email, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BffAdminClinicResponse>> ListClinicsAsync(string subjectId, string? email, CancellationToken cancellationToken);
    Task<BffAdminClinicResponse> CreateClinicAsync(string subjectId, string? email, BffCreateClinicRequest request, CancellationToken cancellationToken);
    Task<BffAdminClinicResponse?> UpdateClinicStatusAsync(string subjectId, string? email, Guid tenantId, BffUpdateClinicStatusRequest request, CancellationToken cancellationToken);
    Task<bool> AddClinicAdminAsync(string subjectId, string? email, Guid tenantId, BffCreateClinicAdminRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BffSystemAdminUserResponse>> ListSystemAdminsAsync(string subjectId, string? email, CancellationToken cancellationToken);
    Task<bool> UpdateSystemAdminAsync(string subjectId, string? email, string targetSubjectId, BffUpdateSystemAdminRequest request, CancellationToken cancellationToken);
}

public sealed class PostgresBffSaaSAdminService(
    IOptions<SystemAdminOptions> options,
    IConfiguration configuration) : IBffSaaSAdminService
{
    private readonly ConcurrentDictionary<Guid, BffAdminClinicResponse> memoryClinics = [];
    private readonly ConcurrentDictionary<string, BffSystemAdminUserResponse> memorySystemAdmins =
        new(StringComparer.OrdinalIgnoreCase);

    public async Task<bool> IsSystemAdminAsync(string? subjectId, string? email, CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("PatientsDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return IsConfiguredSystemAdmin(subjectId, email);
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("select core.is_system_admin(@subject_id, @email);", connection);
        command.Parameters.AddWithValue("subject_id", (object?)subjectId ?? string.Empty);
        command.Parameters.AddWithValue("email", (object?)email ?? string.Empty);
        return await command.ExecuteScalarAsync(cancellationToken) is true;
    }

    public async Task<IReadOnlyCollection<BffAdminClinicResponse>> ListClinicsAsync(string subjectId, string? email, CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("PatientsDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            EnsureMemorySystemAdmins();
            return memoryClinics.Values.OrderBy(clinic => clinic.Name).ToArray();
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            select id, name, active
            from core.admin_list_tenants(@subject_id, @email);
            """, connection);
        command.Parameters.AddWithValue("subject_id", subjectId);
        command.Parameters.AddWithValue("email", (object?)email ?? string.Empty);

        var clinics = new List<BffAdminClinicResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            clinics.Add(new BffAdminClinicResponse(reader.GetGuid(0), reader.GetString(1), reader.GetBoolean(2)));
        }

        return clinics;
    }

    public async Task<BffAdminClinicResponse> CreateClinicAsync(
        string subjectId,
        string? email,
        BffCreateClinicRequest request,
        CancellationToken cancellationToken)
    {
        var name = NormalizeRequired(request.Name, nameof(request.Name), 160);
        var adminSubjectId = NormalizeRequired(request.AdminSubjectId, nameof(request.AdminSubjectId), 160);
        var adminEmail = NormalizeRequired(request.AdminEmail, nameof(request.AdminEmail), 160);
        var tenantId = Guid.NewGuid();
        var connectionString = configuration.GetConnectionString("PatientsDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var clinic = new BffAdminClinicResponse(tenantId, name, true);
            memoryClinics[tenantId] = clinic;
            return clinic;
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            select id, name, active
            from core.admin_create_tenant(@subject_id, @email, @tenant_id, @tenant_name, @admin_subject_id, @admin_email);
            """, connection);
        command.Parameters.AddWithValue("subject_id", subjectId);
        command.Parameters.AddWithValue("email", (object?)email ?? string.Empty);
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("tenant_name", name);
        command.Parameters.AddWithValue("admin_subject_id", adminSubjectId);
        command.Parameters.AddWithValue("admin_email", adminEmail);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Tenant creation did not return a clinic.");
        }

        return new BffAdminClinicResponse(reader.GetGuid(0), reader.GetString(1), reader.GetBoolean(2));
    }

    public async Task<BffAdminClinicResponse?> UpdateClinicStatusAsync(
        string subjectId,
        string? email,
        Guid tenantId,
        BffUpdateClinicStatusRequest request,
        CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("PatientsDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (!memoryClinics.TryGetValue(tenantId, out var clinic))
            {
                return null;
            }

            var updated = clinic with { Active = request.Active };
            memoryClinics[tenantId] = updated;
            return updated;
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            select id, name, active
            from core.admin_set_tenant_active(@subject_id, @email, @tenant_id, @active);
            """, connection);
        command.Parameters.AddWithValue("subject_id", subjectId);
        command.Parameters.AddWithValue("email", (object?)email ?? string.Empty);
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("active", request.Active);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new BffAdminClinicResponse(reader.GetGuid(0), reader.GetString(1), reader.GetBoolean(2));
    }

    public async Task<bool> AddClinicAdminAsync(
        string subjectId,
        string? email,
        Guid tenantId,
        BffCreateClinicAdminRequest request,
        CancellationToken cancellationToken)
    {
        var adminSubjectId = NormalizeRequired(request.SubjectId, nameof(request.SubjectId), 160);
        var adminEmail = NormalizeRequired(request.Email, nameof(request.Email), 160);
        var connectionString = configuration.GetConnectionString("PatientsDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return memoryClinics.ContainsKey(tenantId);
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            select core.admin_upsert_tenant_admin(@subject_id, @email, @tenant_id, @admin_subject_id, @admin_email, @full_name, @phone);
            """, connection);
        command.Parameters.AddWithValue("subject_id", subjectId);
        command.Parameters.AddWithValue("email", (object?)email ?? string.Empty);
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("admin_subject_id", adminSubjectId);
        command.Parameters.AddWithValue("admin_email", adminEmail);
        command.Parameters.AddWithValue("full_name", (object?)request.FullName ?? DBNull.Value);
        command.Parameters.AddWithValue("phone", (object?)request.Phone ?? DBNull.Value);
        return await command.ExecuteScalarAsync(cancellationToken) is true;
    }

    public async Task<IReadOnlyCollection<BffSystemAdminUserResponse>> ListSystemAdminsAsync(
        string subjectId,
        string? email,
        CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("PatientsDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            EnsureMemorySystemAdmins();
            return memorySystemAdmins.Values.OrderBy(user => user.Email).ToArray();
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            select subject_id, email, active
            from core.admin_list_system_admins(@subject_id, @email);
            """, connection);
        command.Parameters.AddWithValue("subject_id", subjectId);
        command.Parameters.AddWithValue("email", (object?)email ?? string.Empty);

        var users = new List<BffSystemAdminUserResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(new BffSystemAdminUserResponse(reader.GetString(0), reader.GetString(1), reader.GetBoolean(2)));
        }

        return users;
    }

    public async Task<bool> UpdateSystemAdminAsync(
        string subjectId,
        string? email,
        string targetSubjectId,
        BffUpdateSystemAdminRequest request,
        CancellationToken cancellationToken)
    {
        var targetEmail = NormalizeRequired(request.Email, nameof(request.Email), 160);
        var connectionString = configuration.GetConnectionString("PatientsDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            EnsureMemorySystemAdmins();
            memorySystemAdmins[targetSubjectId] = new BffSystemAdminUserResponse(targetSubjectId, targetEmail, request.Active);
            return true;
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            select core.admin_upsert_system_admin(@subject_id, @email, @target_subject_id, @target_email, @active);
            """, connection);
        command.Parameters.AddWithValue("subject_id", subjectId);
        command.Parameters.AddWithValue("email", (object?)email ?? string.Empty);
        command.Parameters.AddWithValue("target_subject_id", targetSubjectId);
        command.Parameters.AddWithValue("target_email", targetEmail);
        command.Parameters.AddWithValue("active", request.Active);
        return await command.ExecuteScalarAsync(cancellationToken) is true;
    }

    private bool IsConfiguredSystemAdmin(string? subjectId, string? email)
    {
        EnsureMemorySystemAdmins();
        return options.Value.Users.Any(user =>
            user.Active &&
            (!string.IsNullOrWhiteSpace(subjectId) &&
             string.Equals(user.SubjectId, subjectId, StringComparison.OrdinalIgnoreCase) ||
             !string.IsNullOrWhiteSpace(email) &&
             !string.IsNullOrWhiteSpace(user.Email) &&
             string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)));
    }

    private void EnsureMemorySystemAdmins()
    {
        foreach (var user in options.Value.Users.Where(user => !string.IsNullOrWhiteSpace(user.SubjectId)))
        {
            memorySystemAdmins.TryAdd(
                user.SubjectId,
                new BffSystemAdminUserResponse(user.SubjectId, user.Email, user.Active));
        }
    }

    private static string NormalizeRequired(string value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} is required.", name);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{name} exceeds {maxLength} characters.", name);
        }

        return normalized;
    }
}

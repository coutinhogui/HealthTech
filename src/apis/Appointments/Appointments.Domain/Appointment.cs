using HealthTech.BuildingBlocks.SharedKernel;

namespace HealthTech.Appointments.Domain;

public readonly record struct AppointmentId(Guid Value)
{
    public static AppointmentId New() => new(Guid.NewGuid());
}

public sealed class Appointment : Entity<AppointmentId>
{
    public Guid TenantId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public Guid? LocationId { get; private set; }
    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset EndsAtUtc { get; private set; }
    public string Status { get; private set; }
    public string? Notes { get; private set; }

    private Appointment() : base(new AppointmentId(Guid.Empty))
    {
        Status = "scheduled";
    }

    public Appointment(
        AppointmentId id,
        Guid tenantId,
        Guid patientId,
        Guid professionalId,
        Guid? locationId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        string status,
        string? notes) : base(id)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId obrigatório.", nameof(tenantId));
        if (patientId == Guid.Empty) throw new ArgumentException("PatientId obrigatório.", nameof(patientId));
        if (professionalId == Guid.Empty) throw new ArgumentException("ProfessionalId obrigatório.", nameof(professionalId));
        if (endsAtUtc <= startsAtUtc) throw new ArgumentException("Appointment end must be after start.");
        if (startsAtUtc < DateTimeOffset.UtcNow.AddYears(-1)) throw new ArgumentException("Appointment start is invalid.");

        TenantId = tenantId;
        PatientId = patientId;
        ProfessionalId = professionalId;
        LocationId = locationId;
        StartsAtUtc = startsAtUtc.ToUniversalTime();
        EndsAtUtc = endsAtUtc.ToUniversalTime();
        Status = string.IsNullOrWhiteSpace(status) ? "scheduled" : status.Trim().ToLowerInvariant();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}

using HealthTech.BuildingBlocks.SharedKernel;

namespace HealthTech.Patients.Domain;

public readonly record struct PatientId(Guid Value)
{
    public static PatientId New() => new(Guid.NewGuid());
}

public sealed class Patient : Entity<PatientId>
{
    public Guid TenantId { get; private set; }
    public string FullName { get; private set; }
    public string Document { get; private set; }
    public DateOnly BirthDate { get; private set; }

    private Patient() : base(new PatientId(Guid.Empty))
    {
        FullName = string.Empty;
        Document = string.Empty;
    }

    public Patient(PatientId id, Guid tenantId, string fullName, string document, DateOnly birthDate) : base(id)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId obrigatório.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName obrigatório.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(document)) throw new ArgumentException("Document obrigatório.", nameof(document));
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow)) throw new ArgumentException("BirthDate inválido.", nameof(birthDate));

        TenantId = tenantId;
        FullName = fullName.Trim();
        Document = document.Trim();
        BirthDate = birthDate;
    }
}

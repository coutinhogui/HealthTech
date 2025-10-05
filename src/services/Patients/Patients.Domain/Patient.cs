namespace HealthTech.Patients.Domain;
using HealthTech.BuildingBlocks.SharedKernel;

public readonly record struct PatientId(Guid Value)
{
    public static PatientId New() => new(Guid.NewGuid());
}

public sealed class Patient : Entity<PatientId>
{
    public string FullName { get; private set; }
    public string Document { get; private set; }
    public DateOnly BirthDate { get; private set; }
    
    private Patient() : base(new PatientId(Guid.Empty)) { FullName = Document = string.Empty; BirthDate = default; }

    public Patient(PatientId id, string fullName, string document, DateOnly birthDate) : base(id)
    {

        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName obrigatório");
        if (string.IsNullOrWhiteSpace(document)) throw new ArgumentException("Document obrigatório");
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow)) throw new ArgumentException("BirthDate inválido");
        FullName = fullName; Document = document; BirthDate = BirthDate ;
    }
}

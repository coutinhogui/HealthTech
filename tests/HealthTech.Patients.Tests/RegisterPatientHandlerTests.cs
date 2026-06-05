using HealthTech.BuildingBlocks.Abstractions;
using HealthTech.Patients.Application;
using HealthTech.Patients.Domain;
using Xunit;

namespace HealthTech.Patients.Tests;

public sealed class RegisterPatientHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public async Task Handle_returns_validation_error_when_name_is_missing()
    {
        var repository = new RecordingPatientRepository();
        var handler = new RegisterPatientHandler(
            repository,
            new NoopUnitOfWork(),
            new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new RegisterPatientCommand(" ", "12345678900", new DateOnly(1990, 1, 1)), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("validation_error", result.Error?.Code);
        Assert.Contains(nameof(RegisterPatientCommand.FullName), result.Error!.Details.Keys);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Handle_returns_conflict_when_document_already_exists_for_tenant()
    {
        var repository = new RecordingPatientRepository { DuplicateDocument = true };
        var handler = new RegisterPatientHandler(
            repository,
            new NoopUnitOfWork(),
            new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new RegisterPatientCommand("Maria Silva", "12345678900", new DateOnly(1990, 1, 1)), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("patient_document_conflict", result.Error?.Code);
    }

    private sealed class RecordingPatientRepository : IPatientRepository
    {
        public bool DuplicateDocument { get; init; }
        public List<Patient> Added { get; } = [];

        public Task AddAsync(Patient entity, CancellationToken ct)
        {
            if (DuplicateDocument)
            {
                throw new DuplicatePatientDocumentException(entity.TenantId, entity.Document);
            }

            Added.Add(entity);
            return Task.CompletedTask;
        }

        public Task<Patient?> GetAsync(Guid tenantId, Guid id, CancellationToken ct) => Task.FromResult<Patient?>(null);

        public Task<IReadOnlyCollection<Patient>> ListAsync(Guid tenantId, int skip, int take, CancellationToken ct)
            => Task.FromResult<IReadOnlyCollection<Patient>>([]);

        public Task<IReadOnlyCollection<Patient>> ListForProfessionalAsync(Guid tenantId, Guid professionalId, int skip, int take, CancellationToken ct)
            => Task.FromResult<IReadOnlyCollection<Patient>>([]);

        public Task<bool> PatientHasAppointmentWithProfessionalAsync(Guid tenantId, Guid patientId, Guid professionalId, CancellationToken ct)
            => Task.FromResult(false);

        public Task<Guid?> GetPatientIdForSubjectAsync(Guid tenantId, string subjectId, CancellationToken ct)
            => Task.FromResult<Guid?>(null);

        public Task<Guid> AddInsurancePlanAsync(Guid tenantId, string payerName, string planName, CancellationToken ct)
            => Task.FromResult(Guid.NewGuid());

        public Task<IReadOnlyCollection<InsurancePlanReadModel>> ListInsurancePlansAsync(Guid tenantId, CancellationToken ct)
            => Task.FromResult<IReadOnlyCollection<InsurancePlanReadModel>>([]);

        public Task<bool> PlanExistsAsync(Guid tenantId, Guid planId, CancellationToken ct)
            => Task.FromResult(false);

        public Task UpsertPatientPlanAsync(Guid tenantId, Guid patientId, Guid planId, CancellationToken ct)
            => Task.CompletedTask;

        public Task<IReadOnlyCollection<PatientPlanReadModel>> ListPatientPlansAsync(Guid tenantId, Guid patientId, CancellationToken ct)
            => Task.FromResult<IReadOnlyCollection<PatientPlanReadModel>>([]);

        public Task<Guid> AddPatientEvolutionAsync(
            Guid tenantId,
            Guid patientId,
            DateTimeOffset encounteredAtUtc,
            string subjective,
            string objective,
            string assessment,
            string plan,
            string? createdBySubject,
            CancellationToken ct)
            => Task.FromResult(Guid.NewGuid());

        public Task<IReadOnlyCollection<PatientEvolutionReadModel>> ListPatientEvolutionsAsync(Guid tenantId, Guid patientId, int skip, int take, CancellationToken ct)
            => Task.FromResult<IReadOnlyCollection<PatientEvolutionReadModel>>([]);
    }

    private sealed class NoopUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);
    }

    private sealed class FixedRequestContextAccessor(Guid tenantId) : IRequestContextAccessor
    {
        public ResolvedRequestContext Current { get; } = new(
            "user-1",
            "user@example.com",
            [new TenantMembership(tenantId, "Demo Clinic", "admin")],
            new TenantMembership(tenantId, "Demo Clinic", "admin"));
    }
}

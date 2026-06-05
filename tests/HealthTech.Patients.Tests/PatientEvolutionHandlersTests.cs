using HealthTech.BuildingBlocks.Abstractions;
using HealthTech.Patients.Application;
using HealthTech.Patients.Domain;
using Xunit;

namespace HealthTech.Patients.Tests;

public sealed class PatientEvolutionHandlersTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid PatientId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task Create_evolution_returns_validation_error_when_subjective_is_missing()
    {
        var handler = new CreatePatientEvolutionHandler(
            new FakePatientRepository { ExistingPatient = NewPatient() },
            new NoopUnitOfWork(),
            new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(
            new CreatePatientEvolutionCommand(PatientId, DateTimeOffset.UtcNow, " ", "obj", "assess", "plan"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("validation_error", result.Error?.Code);
    }

    [Fact]
    public async Task Create_evolution_returns_not_found_when_patient_does_not_exist()
    {
        var handler = new CreatePatientEvolutionHandler(
            new FakePatientRepository { ExistingPatient = null },
            new NoopUnitOfWork(),
            new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(
            new CreatePatientEvolutionCommand(PatientId, DateTimeOffset.UtcNow, "subj", "obj", "assess", "plan"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("patient_not_found", result.Error?.Code);
    }

    [Fact]
    public async Task Create_evolution_persists_note_for_tenant_patient()
    {
        var repository = new FakePatientRepository
        {
            ExistingPatient = NewPatient(),
            NextEvolutionId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc")
        };
        var handler = new CreatePatientEvolutionHandler(
            repository,
            new NoopUnitOfWork(),
            new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(
            new CreatePatientEvolutionCommand(PatientId, DateTimeOffset.UtcNow, "subj", "obj", "assess", "plan"),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(repository.NextEvolutionId, result.Value);
        Assert.Single(repository.EvolutionWrites);
    }

    [Fact]
    public async Task List_evolutions_returns_tenant_patient_history()
    {
        var repository = new FakePatientRepository
        {
            ExistingPatient = NewPatient(),
            Evolutions =
            [
                new PatientEvolutionReadModel(
                    Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    TenantId,
                    PatientId,
                    DateTimeOffset.UtcNow,
                    "subj",
                    "obj",
                    "assess",
                    "plan",
                    "user-1",
                    DateTimeOffset.UtcNow)
            ]
        };
        var handler = new ListPatientEvolutionsHandler(repository, new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new ListPatientEvolutionsQuery(PatientId, 0, 50), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task Patient_role_reads_evolutions_for_own_identity_even_when_request_uses_other_patient_id()
    {
        var ownPatientId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        var forgedPatientId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var repository = new FakePatientRepository
        {
            ExistingPatient = new Patient(new PatientId(ownPatientId), TenantId, "Paciente Logado", "999", new DateOnly(1995, 1, 1)),
            PatientIdForSubject = ownPatientId,
            Evolutions =
            [
                new PatientEvolutionReadModel(
                    Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    TenantId,
                    ownPatientId,
                    DateTimeOffset.UtcNow,
                    "subj",
                    "obj",
                    "assess",
                    "plan",
                    "user-1",
                    DateTimeOffset.UtcNow)
            ]
        };
        var handler = new ListPatientEvolutionsHandler(repository, new FixedRequestContextAccessor(TenantId, "patient"));

        var result = await handler.Handle(new ListPatientEvolutionsQuery(forgedPatientId, 0, 50), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(ownPatientId, repository.LastEvolutionPatientId);
        Assert.Single(result.Value!);
    }

    private static Patient NewPatient()
        => new(new PatientId(PatientId), TenantId, "Maria Silva", "123", new DateOnly(1990, 1, 1));

    private sealed class FakePatientRepository : IPatientRepository
    {
        public Patient? ExistingPatient { get; init; }
        public Guid? PatientIdForSubject { get; init; }
        public Guid? LastEvolutionPatientId { get; private set; }
        public Guid NextEvolutionId { get; init; } = Guid.NewGuid();
        public IReadOnlyCollection<PatientEvolutionReadModel> Evolutions { get; init; } = [];
        public List<(Guid TenantId, Guid PatientId, string Subjective, string Objective, string Assessment, string Plan)> EvolutionWrites { get; } = [];

        public Task AddAsync(Patient entity, CancellationToken ct) => Task.CompletedTask;
        public Task<Patient?> GetAsync(Guid tenantId, Guid id, CancellationToken ct) => Task.FromResult(ExistingPatient);
        public Task<IReadOnlyCollection<Patient>> ListAsync(Guid tenantId, int skip, int take, CancellationToken ct) => Task.FromResult<IReadOnlyCollection<Patient>>([]);
        public Task<IReadOnlyCollection<Patient>> ListForProfessionalAsync(Guid tenantId, Guid professionalId, int skip, int take, CancellationToken ct) => Task.FromResult<IReadOnlyCollection<Patient>>([]);
        public Task<bool> PatientHasAppointmentWithProfessionalAsync(Guid tenantId, Guid patientId, Guid professionalId, CancellationToken ct) => Task.FromResult(true);
        public Task<Guid> AddInsurancePlanAsync(Guid tenantId, string payerName, string planName, CancellationToken ct) => Task.FromResult(Guid.NewGuid());
        public Task<IReadOnlyCollection<InsurancePlanReadModel>> ListInsurancePlansAsync(Guid tenantId, CancellationToken ct) => Task.FromResult<IReadOnlyCollection<InsurancePlanReadModel>>([]);
        public Task<bool> PlanExistsAsync(Guid tenantId, Guid planId, CancellationToken ct) => Task.FromResult(false);
        public Task UpsertPatientPlanAsync(Guid tenantId, Guid patientId, Guid planId, CancellationToken ct) => Task.CompletedTask;
        public Task<IReadOnlyCollection<PatientPlanReadModel>> ListPatientPlansAsync(Guid tenantId, Guid patientId, CancellationToken ct) => Task.FromResult<IReadOnlyCollection<PatientPlanReadModel>>([]);
        public Task<Guid?> GetPatientIdForSubjectAsync(Guid tenantId, string subjectId, CancellationToken ct) => Task.FromResult(PatientIdForSubject);

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
        {
            EvolutionWrites.Add((tenantId, patientId, subjective, objective, assessment, plan));
            return Task.FromResult(NextEvolutionId);
        }

        public Task<IReadOnlyCollection<PatientEvolutionReadModel>> ListPatientEvolutionsAsync(Guid tenantId, Guid patientId, int skip, int take, CancellationToken ct)
        {
            LastEvolutionPatientId = patientId;
            return Task.FromResult(Evolutions);
        }
    }

    private sealed class NoopUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);
    }

    private sealed class FixedRequestContextAccessor(Guid tenantId, string role = "admin") : IRequestContextAccessor
    {
        public ResolvedRequestContext Current { get; } = new(
            "user-1",
            "user@example.com",
            [new TenantMembership(tenantId, "Demo Clinic", role)],
            new TenantMembership(tenantId, "Demo Clinic", role));
    }
}

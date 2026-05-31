using HealthTech.BuildingBlocks.Abstractions;
using HealthTech.Patients.Application;
using HealthTech.Patients.Domain;
using Xunit;

namespace HealthTech.Patients.Tests;

public sealed class InsurancePlanHandlersTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid PatientId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid PlanId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Fact]
    public async Task Create_plan_returns_validation_error_when_fields_are_missing()
    {
        var repository = new FakePatientRepository();
        var handler = new CreateInsurancePlanHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new CreateInsurancePlanCommand(" ", " "), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("validation_error", result.Error?.Code);
        Assert.Contains(nameof(CreateInsurancePlanCommand.PayerName), result.Error!.Details.Keys);
        Assert.Contains(nameof(CreateInsurancePlanCommand.PlanName), result.Error.Details.Keys);
    }

    [Fact]
    public async Task Create_plan_returns_conflict_when_duplicate_plan_exists()
    {
        var repository = new FakePatientRepository { DuplicatePlan = true };
        var handler = new CreateInsurancePlanHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new CreateInsurancePlanCommand("Unimed", "Basico"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("plan_conflict", result.Error?.Code);
    }

    [Fact]
    public async Task Assign_plan_returns_patient_not_found_when_patient_does_not_exist()
    {
        var repository = new FakePatientRepository
        {
            ExistingPlans = [PlanId]
        };
        var handler = new AssignPatientPlanHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new AssignPatientPlanCommand(PatientId, PlanId), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("patient_not_found", result.Error?.Code);
    }

    [Fact]
    public async Task Assign_plan_returns_plan_not_found_when_plan_does_not_exist()
    {
        var repository = new FakePatientRepository
        {
            ExistingPatients = [new Patient(new PatientId(PatientId), TenantId, "Maria Silva", "123", new DateOnly(1990, 1, 1))]
        };
        var handler = new AssignPatientPlanHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new AssignPatientPlanCommand(PatientId, PlanId), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("plan_not_found", result.Error?.Code);
    }

    [Fact]
    public async Task Assign_plan_upserts_link_when_patient_and_plan_exist()
    {
        var repository = new FakePatientRepository
        {
            ExistingPatients = [new Patient(new PatientId(PatientId), TenantId, "Maria Silva", "123", new DateOnly(1990, 1, 1))],
            ExistingPlans = [PlanId]
        };
        var handler = new AssignPatientPlanHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new AssignPatientPlanCommand(PatientId, PlanId), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(repository.LinkedPlans);
        Assert.Equal((TenantId, PatientId, PlanId), repository.LinkedPlans[0]);
    }

    private sealed class FakePatientRepository : IPatientRepository
    {
        public bool DuplicatePlan { get; init; }
        public List<Patient> ExistingPatients { get; init; } = [];
        public List<Guid> ExistingPlans { get; init; } = [];
        public List<(Guid TenantId, Guid PatientId, Guid PlanId)> LinkedPlans { get; } = [];

        public Task AddAsync(Patient entity, CancellationToken ct) => Task.CompletedTask;

        public Task<Patient?> GetAsync(Guid tenantId, Guid id, CancellationToken ct)
            => Task.FromResult(ExistingPatients.FirstOrDefault(p => p.TenantId == tenantId && p.Id.Value == id));

        public Task<IReadOnlyCollection<Patient>> ListAsync(Guid tenantId, int skip, int take, CancellationToken ct)
            => Task.FromResult<IReadOnlyCollection<Patient>>(ExistingPatients.Where(p => p.TenantId == tenantId).ToArray());

        public Task<Guid> AddInsurancePlanAsync(Guid tenantId, string payerName, string planName, CancellationToken ct)
        {
            if (DuplicatePlan)
            {
                throw new DuplicateInsurancePlanException(tenantId, payerName, planName);
            }

            var newId = Guid.NewGuid();
            ExistingPlans.Add(newId);
            return Task.FromResult(newId);
        }

        public Task<IReadOnlyCollection<InsurancePlanReadModel>> ListInsurancePlansAsync(Guid tenantId, CancellationToken ct)
            => Task.FromResult<IReadOnlyCollection<InsurancePlanReadModel>>([]);

        public Task<bool> PlanExistsAsync(Guid tenantId, Guid planId, CancellationToken ct)
            => Task.FromResult(ExistingPlans.Contains(planId));

        public Task UpsertPatientPlanAsync(Guid tenantId, Guid patientId, Guid planId, CancellationToken ct)
        {
            LinkedPlans.Add((tenantId, patientId, planId));
            return Task.CompletedTask;
        }

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

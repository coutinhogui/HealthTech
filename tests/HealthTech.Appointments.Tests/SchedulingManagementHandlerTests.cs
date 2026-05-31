using HealthTech.Appointments.Application;
using HealthTech.Appointments.Domain;
using HealthTech.BuildingBlocks.Abstractions;
using Xunit;

namespace HealthTech.Appointments.Tests;

public sealed class SchedulingManagementHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid SpecialtyId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ProfessionalId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Fact]
    public async Task Create_specialty_rejects_empty_name()
    {
        var handler = new CreateSpecialtyHandler(new RecordingSchedulingRepository(), new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new CreateSpecialtyCommand(" "), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("validation_error", result.Error?.Code);
    }

    [Fact]
    public async Task Create_professional_rejects_unknown_specialty()
    {
        var repository = new RecordingSchedulingRepository { SpecialtyExists = false };
        var handler = new CreateProfessionalHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new CreateProfessionalCommand("Dra. Helena Costa", SpecialtyId), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("specialty_not_found", result.Error?.Code);
        Assert.Empty(repository.ProfessionalCreates);
    }

    [Fact]
    public async Task Create_professional_adds_professional_for_active_tenant()
    {
        var repository = new RecordingSchedulingRepository { SpecialtyExists = true, NextProfessionalId = ProfessionalId };
        var handler = new CreateProfessionalHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new CreateProfessionalCommand("Dra. Helena Costa", SpecialtyId), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(ProfessionalId, result.Value);
        var create = Assert.Single(repository.ProfessionalCreates);
        Assert.Equal(TenantId, create.TenantId);
        Assert.Equal("Dra. Helena Costa", create.FullName);
        Assert.Equal(SpecialtyId, create.SpecialtyId);
    }

    [Fact]
    public async Task List_locations_returns_tenant_locations()
    {
        var locationId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var repository = new RecordingSchedulingRepository
        {
            Locations =
            [
                new LocationReadModel(locationId, "Unidade principal", "America/Sao_Paulo", true)
            ]
        };
        var handler = new ListLocationsHandler(repository, new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new ListLocationsQuery(), CancellationToken.None);

        Assert.True(result.Success);
        var location = Assert.Single(result.Value!);
        Assert.Equal(locationId, location.Id);
        Assert.Equal("Unidade principal", location.Name);
    }

    private sealed class RecordingSchedulingRepository : IAppointmentRepository
    {
        public bool SpecialtyExists { get; init; }
        public Guid NextSpecialtyId { get; init; } = Guid.NewGuid();
        public Guid NextLocationId { get; init; } = Guid.NewGuid();
        public Guid NextProfessionalId { get; init; } = Guid.NewGuid();
        public List<(Guid TenantId, string Name)> SpecialtyCreates { get; } = [];
        public List<(Guid TenantId, string Name, string Timezone)> LocationCreates { get; } = [];
        public List<(Guid TenantId, string FullName, Guid SpecialtyId)> ProfessionalCreates { get; } = [];
        public IReadOnlyCollection<LocationReadModel> Locations { get; init; } = [];

        public Task AddAsync(Appointment appointment, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> PatientExistsAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> ProfessionalExistsAsync(Guid tenantId, Guid professionalId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> HasProfessionalConflictAsync(Guid tenantId, Guid professionalId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> HasProfessionalConflictAsync(Guid tenantId, Guid professionalId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, Guid? excludingAppointmentId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> LocationExistsAsync(Guid tenantId, Guid locationId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> CancelAsync(Guid tenantId, Guid appointmentId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> RescheduleAsync(Guid tenantId, Guid appointmentId, Guid professionalId, Guid? locationId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, string? notes, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<IReadOnlyCollection<Appointment>> ListAsync(Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<Appointment>>([]);
        public Task<IReadOnlyCollection<BusyWindowReadModel>> ListBusyWindowsAsync(Guid tenantId, Guid professionalId, Guid? locationId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<BusyWindowReadModel>>([]);
        public Task<IReadOnlyCollection<ProfessionalReadModel>> ListProfessionalsAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<ProfessionalReadModel>>([]);
        public Task<IReadOnlyCollection<SpecialtyReadModel>> ListSpecialtiesAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<SpecialtyReadModel>>([]);
        public Task<IReadOnlyCollection<LocationReadModel>> ListLocationsAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(Locations);
        public Task<bool> SpecialtyExistsAsync(Guid tenantId, Guid specialtyId, CancellationToken cancellationToken) => Task.FromResult(SpecialtyExists);

        public Task<Guid> AddSpecialtyAsync(Guid tenantId, string name, CancellationToken cancellationToken)
        {
            SpecialtyCreates.Add((tenantId, name));
            return Task.FromResult(NextSpecialtyId);
        }

        public Task<Guid> AddLocationAsync(Guid tenantId, string name, string timezone, CancellationToken cancellationToken)
        {
            LocationCreates.Add((tenantId, name, timezone));
            return Task.FromResult(NextLocationId);
        }

        public Task<Guid> AddProfessionalAsync(Guid tenantId, string fullName, Guid specialtyId, CancellationToken cancellationToken)
        {
            ProfessionalCreates.Add((tenantId, fullName, specialtyId));
            return Task.FromResult(NextProfessionalId);
        }

        public Task<bool> AppointmentExistsAsync(Guid tenantId, Guid appointmentId, CancellationToken cancellationToken)
            => Task.FromResult(true);

        public Task<Guid> AddManualWhatsappIntentAsync(
            Guid tenantId,
            Guid appointmentId,
            string recipientPhone,
            string messageText,
            string? createdBySubject,
            CancellationToken cancellationToken)
            => Task.FromResult(Guid.NewGuid());

        public Task<bool> MarkManualWhatsappSentAsync(
            Guid tenantId,
            Guid appointmentId,
            Guid messageId,
            string? provider,
            string? providerMessageId,
            DateTimeOffset sentAtUtc,
            CancellationToken cancellationToken)
            => Task.FromResult(true);

        public Task<IReadOnlyCollection<ManualWhatsappMessageReadModel>> ListManualWhatsappMessagesAsync(
            Guid tenantId,
            Guid appointmentId,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<ManualWhatsappMessageReadModel>>([]);

        public Task<Guid> AddChargeAsync(
            Guid tenantId,
            Guid appointmentId,
            Guid patientId,
            string payerType,
            string description,
            decimal amount,
            DateOnly dueDate,
            string? externalReference,
            CancellationToken cancellationToken)
            => Task.FromResult(Guid.NewGuid());

        public Task<bool> MarkChargeAsPaidAsync(
            Guid tenantId,
            Guid chargeId,
            DateTimeOffset paidAtUtc,
            CancellationToken cancellationToken)
            => Task.FromResult(false);

        public Task<IReadOnlyCollection<ChargeReadModel>> ListChargesAsync(
            Guid tenantId,
            string? status,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<ChargeReadModel>>([]);

        public Task<Guid> CloseBatchAsync(
            Guid tenantId,
            DateOnly period,
            string? closedBySubject,
            CancellationToken cancellationToken)
            => Task.FromResult(Guid.NewGuid());

        public Task<IReadOnlyCollection<BillingBatchReadModel>> ListBatchesAsync(
            Guid tenantId,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<BillingBatchReadModel>>([]);
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

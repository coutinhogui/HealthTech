using HealthTech.Appointments.Application;
using HealthTech.Appointments.Domain;
using HealthTech.BuildingBlocks.Abstractions;
using Xunit;

namespace HealthTech.Appointments.Tests;

public sealed class CreateAppointmentHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid PatientId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid ProfessionalId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Fact]
    public async Task Handle_returns_validation_error_when_time_window_is_invalid()
    {
        var repository = new RecordingAppointmentRepository { PatientExists = true, ProfessionalExists = true };
        var handler = new CreateAppointmentHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));
        var startsAt = DateTimeOffset.UtcNow.AddDays(1);

        var result = await handler.Handle(
            new CreateAppointmentCommand(PatientId, ProfessionalId, null, startsAt, startsAt.AddMinutes(-10), null),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("validation_error", result.Error?.Code);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Handle_returns_not_found_when_patient_does_not_belong_to_tenant()
    {
        var repository = new RecordingAppointmentRepository { PatientExists = false, ProfessionalExists = true };
        var handler = new CreateAppointmentHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("patient_not_found", result.Error?.Code);
    }

    [Fact]
    public async Task Handle_returns_conflict_when_professional_has_overlapping_appointment()
    {
        var repository = new RecordingAppointmentRepository
        {
            PatientExists = true,
            ProfessionalExists = true,
            HasConflict = true
        };
        var handler = new CreateAppointmentHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("appointment_conflict", result.Error?.Code);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Handle_returns_not_found_when_location_does_not_belong_to_tenant()
    {
        var repository = new RecordingAppointmentRepository
        {
            PatientExists = true,
            ProfessionalExists = true,
            LocationExists = false
        };
        var handler = new CreateAppointmentHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(ValidCommand() with { LocationId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd") }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("location_not_found", result.Error?.Code);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Cancel_marks_tenant_appointment_as_cancelled()
    {
        var repository = new RecordingAppointmentRepository { CancelResult = true };
        var handler = new CancelAppointmentHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new CancelAppointmentCommand(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee")), CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(result.Value);
        Assert.Equal(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), repository.CancelledAppointmentId);
    }

    [Fact]
    public async Task Reschedule_returns_conflict_when_new_window_overlaps()
    {
        var repository = new RecordingAppointmentRepository
        {
            ProfessionalExists = true,
            HasConflict = true
        };
        var handler = new RescheduleAppointmentHandler(repository, new NoopUnitOfWork(), new FixedRequestContextAccessor(TenantId));
        var startsAt = DateTimeOffset.UtcNow.AddDays(1);

        var result = await handler.Handle(
            new RescheduleAppointmentCommand(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), ProfessionalId, null, startsAt, startsAt.AddMinutes(30), null),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("appointment_conflict", result.Error?.Code);
        Assert.Null(repository.RescheduledAppointmentId);
    }

    [Fact]
    public async Task List_professionals_returns_active_tenant_professionals()
    {
        var repository = new RecordingAppointmentRepository
        {
            Professionals =
            [
                new ProfessionalReadModel(ProfessionalId, "Dra. Helena Costa", Guid.Parse("22222222-2222-2222-2222-222222222222"), "Clinica Geral", true)
            ]
        };
        var handler = new ListProfessionalsHandler(repository, new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new ListProfessionalsQuery(), CancellationToken.None);

        Assert.True(result.Success);
        var professional = Assert.Single(result.Value!);
        Assert.Equal(ProfessionalId, professional.Id);
        Assert.Equal("Dra. Helena Costa", professional.FullName);
    }

    [Fact]
    public async Task List_available_slots_returns_slot_grid_excluding_busy_windows()
    {
        var startsAt = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero);
        var repository = new RecordingAppointmentRepository
        {
            ProfessionalExists = true,
            BusyWindows =
            [
                new BusyWindowReadModel(startsAt.AddMinutes(30), startsAt.AddMinutes(60)),
                new BusyWindowReadModel(startsAt.AddMinutes(90), startsAt.AddMinutes(120))
            ]
        };
        var handler = new ListAvailableSlotsHandler(repository, new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(
            new ListAvailableSlotsQuery(ProfessionalId, startsAt, startsAt.AddHours(2), 30, null),
            CancellationToken.None);

        Assert.True(result.Success);
        var slots = Assert.IsAssignableFrom<IReadOnlyCollection<AvailableSlotReadModel>>(result.Value);
        Assert.Equal(2, slots.Count);
        Assert.Equal(startsAt, slots.ElementAt(0).StartsAtUtc);
        Assert.Equal(startsAt.AddMinutes(60), slots.ElementAt(1).StartsAtUtc);
    }

    [Fact]
    public async Task List_available_slots_returns_not_found_for_unknown_professional()
    {
        var handler = new ListAvailableSlotsHandler(
            new RecordingAppointmentRepository { ProfessionalExists = false },
            new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(
            new ListAvailableSlotsQuery(ProfessionalId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), 30, null),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("professional_not_found", result.Error?.Code);
    }

    private static CreateAppointmentCommand ValidCommand()
    {
        var startsAt = DateTimeOffset.UtcNow.AddDays(1);
        return new CreateAppointmentCommand(PatientId, ProfessionalId, null, startsAt, startsAt.AddMinutes(30), "Consulta inicial");
    }

    private sealed class RecordingAppointmentRepository : IAppointmentRepository
    {
        public bool PatientExists { get; init; }
        public bool ProfessionalExists { get; init; }
        public bool LocationExists { get; init; } = true;
        public bool HasConflict { get; init; }
        public bool CancelResult { get; init; }
        public Guid? CancelledAppointmentId { get; private set; }
        public Guid? RescheduledAppointmentId { get; private set; }
        public List<Appointment> Added { get; } = [];
        public IReadOnlyCollection<ProfessionalReadModel> Professionals { get; init; } = [];
        public IReadOnlyCollection<BusyWindowReadModel> BusyWindows { get; init; } = [];

        public Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
        {
            Added.Add(appointment);
            return Task.CompletedTask;
        }

        public Task<bool> PatientExistsAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken)
            => Task.FromResult(PatientExists);

        public Task<bool> ProfessionalExistsAsync(Guid tenantId, Guid professionalId, CancellationToken cancellationToken)
            => Task.FromResult(ProfessionalExists);

        public Task<bool> HasProfessionalConflictAsync(
            Guid tenantId,
            Guid professionalId,
            DateTimeOffset startsAtUtc,
            DateTimeOffset endsAtUtc,
            Guid? excludingAppointmentId,
            CancellationToken cancellationToken)
            => Task.FromResult(HasConflict);

        public Task<bool> LocationExistsAsync(Guid tenantId, Guid locationId, CancellationToken cancellationToken)
            => Task.FromResult(LocationExists);

        public Task<bool> CancelAsync(Guid tenantId, Guid appointmentId, CancellationToken cancellationToken)
        {
            CancelledAppointmentId = appointmentId;
            return Task.FromResult(CancelResult);
        }

        public Task<bool> RescheduleAsync(
            Guid tenantId,
            Guid appointmentId,
            Guid professionalId,
            Guid? locationId,
            DateTimeOffset startsAtUtc,
            DateTimeOffset endsAtUtc,
            string? notes,
            CancellationToken cancellationToken)
        {
            RescheduledAppointmentId = appointmentId;
            return Task.FromResult(true);
        }

        public Task<IReadOnlyCollection<Appointment>> ListAsync(Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<Appointment>>([]);

        public Task<IReadOnlyCollection<BusyWindowReadModel>> ListBusyWindowsAsync(
            Guid tenantId,
            Guid professionalId,
            Guid? locationId,
            DateTimeOffset fromUtc,
            DateTimeOffset toUtc,
            CancellationToken cancellationToken)
            => Task.FromResult(BusyWindows);

        public Task<IReadOnlyCollection<ProfessionalReadModel>> ListProfessionalsAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult(Professionals);

        public Task<IReadOnlyCollection<SpecialtyReadModel>> ListSpecialtiesAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<SpecialtyReadModel>>([]);

        public Task<IReadOnlyCollection<LocationReadModel>> ListLocationsAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<LocationReadModel>>([]);

        public Task<bool> SpecialtyExistsAsync(Guid tenantId, Guid specialtyId, CancellationToken cancellationToken)
            => Task.FromResult(true);

        public Task<Guid> AddSpecialtyAsync(Guid tenantId, string name, CancellationToken cancellationToken)
            => Task.FromResult(Guid.NewGuid());

        public Task<Guid> AddLocationAsync(Guid tenantId, string name, string timezone, CancellationToken cancellationToken)
            => Task.FromResult(Guid.NewGuid());

        public Task<Guid> AddProfessionalAsync(Guid tenantId, string fullName, Guid specialtyId, CancellationToken cancellationToken)
            => Task.FromResult(Guid.NewGuid());

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
            string? createdBySubject,
            CancellationToken cancellationToken)
            => Task.FromResult(Guid.NewGuid());

        public Task<bool> MarkChargeAsPaidAsync(Guid tenantId, Guid chargeId, DateTimeOffset paidAtUtc, CancellationToken cancellationToken)
            => Task.FromResult(true);

        public Task<IReadOnlyCollection<ChargeReadModel>> ListChargesAsync(Guid tenantId, string? status, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<ChargeReadModel>>([]);

        public Task<Guid> CloseBatchAsync(Guid tenantId, DateOnly competenceMonth, string? createdBySubject, CancellationToken cancellationToken)
            => Task.FromResult(Guid.NewGuid());

        public Task<IReadOnlyCollection<BillingBatchReadModel>> ListBatchesAsync(Guid tenantId, CancellationToken cancellationToken)
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

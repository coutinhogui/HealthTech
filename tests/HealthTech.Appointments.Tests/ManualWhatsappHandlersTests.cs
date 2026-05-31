using HealthTech.Appointments.Application;
using HealthTech.Appointments.Domain;
using HealthTech.BuildingBlocks.Abstractions;
using Xunit;

namespace HealthTech.Appointments.Tests;

public sealed class ManualWhatsappHandlersTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid AppointmentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task Register_intent_returns_validation_error_when_phone_is_missing()
    {
        var handler = new RegisterManualWhatsappIntentHandler(
            new RecordingWhatsappRepository { AppointmentExists = true },
            new NoopUnitOfWork(),
            new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(
            new RegisterManualWhatsappIntentCommand(AppointmentId, " ", "Mensagem"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("validation_error", result.Error?.Code);
    }

    [Fact]
    public async Task Register_intent_returns_not_found_when_appointment_is_unknown()
    {
        var handler = new RegisterManualWhatsappIntentHandler(
            new RecordingWhatsappRepository { AppointmentExists = false },
            new NoopUnitOfWork(),
            new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(
            new RegisterManualWhatsappIntentCommand(AppointmentId, "+5511999999999", "Mensagem"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("appointment_not_found", result.Error?.Code);
    }

    [Fact]
    public async Task Register_intent_persists_manual_whatsapp_record()
    {
        var repository = new RecordingWhatsappRepository
        {
            AppointmentExists = true,
            NextMessageId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc")
        };
        var handler = new RegisterManualWhatsappIntentHandler(
            repository,
            new NoopUnitOfWork(),
            new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(
            new RegisterManualWhatsappIntentCommand(AppointmentId, "+5511999999999", "Confirmacao manual"),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(repository.NextMessageId, result.Value);
        var write = Assert.Single(repository.IntentWrites);
        Assert.Equal(TenantId, write.TenantId);
        Assert.Equal(AppointmentId, write.AppointmentId);
        Assert.Equal("+5511999999999", write.RecipientPhone);
    }

    [Fact]
    public async Task Mark_sent_returns_not_found_when_message_is_unknown()
    {
        var handler = new MarkManualWhatsappSentHandler(
            new RecordingWhatsappRepository
            {
                AppointmentExists = true,
                MarkSentResult = false
            },
            new NoopUnitOfWork(),
            new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(
            new MarkManualWhatsappSentCommand(
                AppointmentId,
                Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                "whatsapp-cloud-api",
                "provider-id"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("whatsapp_message_not_found", result.Error?.Code);
    }

    [Fact]
    public async Task List_messages_returns_tenant_messages_for_appointment()
    {
        var now = DateTimeOffset.UtcNow;
        var repository = new RecordingWhatsappRepository
        {
            AppointmentExists = true,
            Messages =
            [
                new ManualWhatsappMessageReadModel(
                    Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                    AppointmentId,
                    "+5511999999999",
                    "Confirmacao",
                    "intent",
                    null,
                    null,
                    "user-1",
                    now,
                    null)
            ]
        };
        var handler = new ListManualWhatsappMessagesHandler(repository, new FixedRequestContextAccessor(TenantId));

        var result = await handler.Handle(new ListManualWhatsappMessagesQuery(AppointmentId), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Value!);
    }

    private sealed class RecordingWhatsappRepository : IAppointmentRepository
    {
        public bool AppointmentExists { get; init; }
        public bool MarkSentResult { get; init; } = true;
        public Guid NextMessageId { get; init; } = Guid.NewGuid();
        public List<(Guid TenantId, Guid AppointmentId, string RecipientPhone, string MessageText, string? SubjectId)> IntentWrites { get; } = [];
        public IReadOnlyCollection<ManualWhatsappMessageReadModel> Messages { get; init; } = [];

        public Task AddAsync(Appointment appointment, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> PatientExistsAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> ProfessionalExistsAsync(Guid tenantId, Guid professionalId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> LocationExistsAsync(Guid tenantId, Guid locationId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> HasProfessionalConflictAsync(Guid tenantId, Guid professionalId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, Guid? excludingAppointmentId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> CancelAsync(Guid tenantId, Guid appointmentId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> RescheduleAsync(Guid tenantId, Guid appointmentId, Guid professionalId, Guid? locationId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, string? notes, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<IReadOnlyCollection<Appointment>> ListAsync(Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<Appointment>>([]);
        public Task<IReadOnlyCollection<BusyWindowReadModel>> ListBusyWindowsAsync(Guid tenantId, Guid professionalId, Guid? locationId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<BusyWindowReadModel>>([]);
        public Task<IReadOnlyCollection<SpecialtyReadModel>> ListSpecialtiesAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<SpecialtyReadModel>>([]);
        public Task<IReadOnlyCollection<LocationReadModel>> ListLocationsAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<LocationReadModel>>([]);
        public Task<IReadOnlyCollection<ProfessionalReadModel>> ListProfessionalsAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<ProfessionalReadModel>>([]);
        public Task<bool> SpecialtyExistsAsync(Guid tenantId, Guid specialtyId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<Guid> AddSpecialtyAsync(Guid tenantId, string name, CancellationToken cancellationToken) => Task.FromResult(Guid.NewGuid());
        public Task<Guid> AddLocationAsync(Guid tenantId, string name, string timezone, CancellationToken cancellationToken) => Task.FromResult(Guid.NewGuid());
        public Task<Guid> AddProfessionalAsync(Guid tenantId, string fullName, Guid specialtyId, CancellationToken cancellationToken) => Task.FromResult(Guid.NewGuid());
        public Task<bool> AppointmentExistsAsync(Guid tenantId, Guid appointmentId, CancellationToken cancellationToken) => Task.FromResult(AppointmentExists);

        public Task<Guid> AddManualWhatsappIntentAsync(
            Guid tenantId,
            Guid appointmentId,
            string recipientPhone,
            string messageText,
            string? createdBySubject,
            CancellationToken cancellationToken)
        {
            IntentWrites.Add((tenantId, appointmentId, recipientPhone, messageText, createdBySubject));
            return Task.FromResult(NextMessageId);
        }

        public Task<bool> MarkManualWhatsappSentAsync(
            Guid tenantId,
            Guid appointmentId,
            Guid messageId,
            string? provider,
            string? providerMessageId,
            DateTimeOffset sentAtUtc,
            CancellationToken cancellationToken)
            => Task.FromResult(MarkSentResult);

        public Task<IReadOnlyCollection<ManualWhatsappMessageReadModel>> ListManualWhatsappMessagesAsync(
            Guid tenantId,
            Guid appointmentId,
            CancellationToken cancellationToken)
            => Task.FromResult(Messages);

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

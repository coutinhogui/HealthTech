using System.Diagnostics.Metrics;
using HealthTech.Appointments.Domain;
using HealthTech.BuildingBlocks.Abstractions;
using HealthTech.BuildingBlocks.SharedKernel;
using MediatR;

namespace HealthTech.Appointments.Application;

public sealed record AppointmentReadModel(
    Guid Id,
    Guid TenantId,
    Guid PatientId,
    Guid ProfessionalId,
    Guid? LocationId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string Status,
    string? Notes);

public sealed record SpecialtyReadModel(Guid Id, string Name, bool Active);
public sealed record LocationReadModel(Guid Id, string Name, string Timezone, bool Active);
public sealed record ProfessionalReadModel(Guid Id, string FullName, Guid SpecialtyId, string SpecialtyName, bool Active);
public sealed record ManualWhatsappMessageReadModel(
    Guid Id,
    Guid AppointmentId,
    string RecipientPhone,
    string MessageText,
    string Status,
    string? Provider,
    string? ProviderMessageId,
    string? CreatedBySubject,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? SentAtUtc);
public sealed record ChargeReadModel(
    Guid Id,
    Guid TenantId,
    Guid AppointmentId,
    Guid PatientId,
    string PayerType,
    string Description,
    decimal Amount,
    DateOnly DueDate,
    string Status,
    DateTimeOffset? PaidAtUtc,
    Guid? BatchId,
    DateTimeOffset CreatedAtUtc);
public sealed record BillingBatchReadModel(
    Guid Id,
    Guid TenantId,
    DateOnly CompetenceMonth,
    decimal TotalAmount,
    string Status,
    DateTimeOffset CreatedAtUtc);
public sealed record BusyWindowReadModel(DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc);
public sealed record AvailableSlotReadModel(
    Guid ProfessionalId,
    Guid? LocationId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);

public record CreateAppointmentCommand(
    Guid PatientId,
    Guid ProfessionalId,
    Guid? LocationId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Notes) : IRequest<Result<Guid>>;

public record RescheduleAppointmentCommand(
    Guid AppointmentId,
    Guid ProfessionalId,
    Guid? LocationId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Notes) : IRequest<Result<bool>>;

public record CancelAppointmentCommand(Guid AppointmentId) : IRequest<Result<bool>>;

public record ListAppointmentsQuery(DateTimeOffset? FromUtc = null, DateTimeOffset? ToUtc = null) : IRequest<Result<IReadOnlyCollection<AppointmentReadModel>>>;
public record ListAvailableSlotsQuery(
    Guid ProfessionalId,
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null,
    int SlotMinutes = 30,
    Guid? LocationId = null) : IRequest<Result<IReadOnlyCollection<AvailableSlotReadModel>>>;
public record ListSpecialtiesQuery : IRequest<Result<IReadOnlyCollection<SpecialtyReadModel>>>;
public record ListLocationsQuery : IRequest<Result<IReadOnlyCollection<LocationReadModel>>>;
public record ListProfessionalsQuery : IRequest<Result<IReadOnlyCollection<ProfessionalReadModel>>>;
public record CreateSpecialtyCommand(string Name) : IRequest<Result<Guid>>;
public record CreateLocationCommand(string Name, string Timezone) : IRequest<Result<Guid>>;
public record CreateProfessionalCommand(string FullName, Guid SpecialtyId) : IRequest<Result<Guid>>;
public record RegisterManualWhatsappIntentCommand(Guid AppointmentId, string RecipientPhone, string MessageText) : IRequest<Result<Guid>>;
public record MarkManualWhatsappSentCommand(Guid AppointmentId, Guid MessageId, string? Provider, string? ProviderMessageId) : IRequest<Result<bool>>;
public record ListManualWhatsappMessagesQuery(Guid AppointmentId) : IRequest<Result<IReadOnlyCollection<ManualWhatsappMessageReadModel>>>;
public record CreateChargeCommand(
    Guid AppointmentId,
    Guid PatientId,
    string PayerType,
    string Description,
    decimal Amount,
    DateOnly DueDate) : IRequest<Result<Guid>>;
public record MarkChargeAsPaidCommand(Guid ChargeId) : IRequest<Result<bool>>;
public record CloseBillingBatchCommand(DateOnly CompetenceMonth) : IRequest<Result<Guid>>;
public record ListChargesQuery(string? Status = null) : IRequest<Result<IReadOnlyCollection<ChargeReadModel>>>;
public record ListBillingBatchesQuery : IRequest<Result<IReadOnlyCollection<BillingBatchReadModel>>>;

public interface IAppointmentRepository
{
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken);
    Task<bool> PatientExistsAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken);
    Task<Guid?> GetPatientIdForSubjectAsync(Guid tenantId, string subjectId, CancellationToken cancellationToken);
    Task<bool> ProfessionalExistsAsync(Guid tenantId, Guid professionalId, CancellationToken cancellationToken);
    Task<bool> LocationExistsAsync(Guid tenantId, Guid locationId, CancellationToken cancellationToken);
    Task<bool> HasProfessionalConflictAsync(Guid tenantId, Guid professionalId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, Guid? excludingAppointmentId, CancellationToken cancellationToken);
    Task<bool> CancelAsync(Guid tenantId, Guid appointmentId, CancellationToken cancellationToken);
    Task<bool> RescheduleAsync(Guid tenantId, Guid appointmentId, Guid professionalId, Guid? locationId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, string? notes, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Appointment>> ListAsync(Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Appointment>> ListForProfessionalAsync(Guid tenantId, Guid professionalId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Appointment>> ListForPatientAsync(Guid tenantId, Guid patientId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BusyWindowReadModel>> ListBusyWindowsAsync(
        Guid tenantId,
        Guid professionalId,
        Guid? locationId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SpecialtyReadModel>> ListSpecialtiesAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LocationReadModel>> ListLocationsAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProfessionalReadModel>> ListProfessionalsAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<bool> SpecialtyExistsAsync(Guid tenantId, Guid specialtyId, CancellationToken cancellationToken);
    Task<Guid> AddSpecialtyAsync(Guid tenantId, string name, CancellationToken cancellationToken);
    Task<Guid> AddLocationAsync(Guid tenantId, string name, string timezone, CancellationToken cancellationToken);
    Task<Guid> AddProfessionalAsync(Guid tenantId, string fullName, Guid specialtyId, CancellationToken cancellationToken);
    Task<bool> AppointmentExistsAsync(Guid tenantId, Guid appointmentId, CancellationToken cancellationToken);
    Task<Guid> AddManualWhatsappIntentAsync(
        Guid tenantId,
        Guid appointmentId,
        string recipientPhone,
        string messageText,
        string? createdBySubject,
        CancellationToken cancellationToken);
    Task<bool> MarkManualWhatsappSentAsync(
        Guid tenantId,
        Guid appointmentId,
        Guid messageId,
        string? provider,
        string? providerMessageId,
        DateTimeOffset sentAtUtc,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ManualWhatsappMessageReadModel>> ListManualWhatsappMessagesAsync(
        Guid tenantId,
        Guid appointmentId,
        CancellationToken cancellationToken);
    Task<Guid> AddChargeAsync(
        Guid tenantId,
        Guid appointmentId,
        Guid patientId,
        string payerType,
        string description,
        decimal amount,
        DateOnly dueDate,
        string? createdBySubject,
        CancellationToken cancellationToken);
    Task<bool> MarkChargeAsPaidAsync(Guid tenantId, Guid chargeId, DateTimeOffset paidAtUtc, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ChargeReadModel>> ListChargesAsync(Guid tenantId, string? status, CancellationToken cancellationToken);
    Task<Guid> CloseBatchAsync(Guid tenantId, DateOnly competenceMonth, string? createdBySubject, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BillingBatchReadModel>> ListBatchesAsync(Guid tenantId, CancellationToken cancellationToken);
}

public sealed class CreateAppointmentHandler(
    IAppointmentRepository repository,
    IUnitOfWork unitOfWork,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
{
    private static readonly Counter<long> AppointmentsBooked =
        HealthTechTelemetry.Meter.CreateCounter<long>("healthtech.appointments.created");

    public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return Result<Guid>.Validation(validationErrors);
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<Guid>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageSchedule) &&
            !context.HasPermission(ClinicPermission.ManageOwnAppointments))
        {
            return Result<Guid>.Fail("role_forbidden", "The active clinic role cannot create appointments.");
        }

        var tenantId = context.GetRequiredTenantId();
        var patientId = await AppointmentPatientIdentityGuards.ResolvePatientIdAsync(context, repository, tenantId, request.PatientId, cancellationToken);
        if (patientId.Error is not null)
        {
            return Result<Guid>.Fail(patientId.Error.Value.Code, patientId.Error.Value.Message);
        }

        if (!await repository.PatientExistsAsync(tenantId, patientId.Value, cancellationToken))
        {
            return Result<Guid>.Fail("patient_not_found", "Patient was not found for the active tenant.");
        }

        if (!await repository.ProfessionalExistsAsync(tenantId, request.ProfessionalId, cancellationToken))
        {
            return Result<Guid>.Fail("professional_not_found", "Professional was not found for the active tenant.");
        }

        if (request.LocationId is not null && !await repository.LocationExistsAsync(tenantId, request.LocationId.Value, cancellationToken))
        {
            return Result<Guid>.Fail("location_not_found", "Location was not found for the active tenant.");
        }

        if (await repository.HasProfessionalConflictAsync(tenantId, request.ProfessionalId, request.StartsAtUtc, request.EndsAtUtc, null, cancellationToken))
        {
            return Result<Guid>.Fail("appointment_conflict", "The professional already has an appointment in this time window.");
        }

        using var activity = HealthTechTelemetry.ActivitySource.StartActivity("appointments.create");
        activity?.SetTag("tenant.id", tenantId);
        activity?.SetTag("professional.id", request.ProfessionalId);

        var appointment = new Appointment(
            AppointmentId.New(),
            tenantId,
            patientId.Value,
            request.ProfessionalId,
            request.LocationId,
            request.StartsAtUtc,
            request.EndsAtUtc,
            "scheduled",
            request.Notes);

        await repository.AddAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        AppointmentsBooked.Add(1, new KeyValuePair<string, object?>("tenant.id", tenantId));

        return Result<Guid>.Ok(appointment.Id.Value);
    }

    private static Dictionary<string, string[]> Validate(CreateAppointmentCommand request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.PatientId == Guid.Empty)
        {
            errors[nameof(CreateAppointmentCommand.PatientId)] = ["Patient id is required."];
        }

        if (request.ProfessionalId == Guid.Empty)
        {
            errors[nameof(CreateAppointmentCommand.ProfessionalId)] = ["Professional id is required."];
        }

        if (request.EndsAtUtc <= request.StartsAtUtc)
        {
            errors[nameof(CreateAppointmentCommand.EndsAtUtc)] = ["Appointment end must be after start."];
        }

        if (request.StartsAtUtc < DateTimeOffset.UtcNow.AddYears(-1))
        {
            errors[nameof(CreateAppointmentCommand.StartsAtUtc)] = ["Appointment start is invalid."];
        }

        return errors;
    }
}

public sealed class ListAppointmentsHandler(
    IAppointmentRepository repository,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<ListAppointmentsQuery, Result<IReadOnlyCollection<AppointmentReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<AppointmentReadModel>>> Handle(ListAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<IReadOnlyCollection<AppointmentReadModel>>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageSchedule) &&
            !context.HasPermission(ClinicPermission.ManageBilling) &&
            !context.HasPermission(ClinicPermission.ReadOwnClinicalSchedule) &&
            !context.HasPermission(ClinicPermission.ManageOwnAppointments))
        {
            return Result<IReadOnlyCollection<AppointmentReadModel>>.Fail("role_forbidden", "The active clinic role cannot list appointments.");
        }

        var fromUtc = request.FromUtc ?? new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var toUtc = request.ToUtc ?? fromUtc.AddDays(7);

        var tenantId = context.GetRequiredTenantId();
        IReadOnlyCollection<Appointment> appointments;
        if (context.IsPatientScoped())
        {
            var patientId = await AppointmentPatientIdentityGuards.ResolvePatientIdAsync(context, repository, tenantId, Guid.Empty, cancellationToken);
            if (patientId.Error is not null)
            {
                return Result<IReadOnlyCollection<AppointmentReadModel>>.Fail(patientId.Error.Value.Code, patientId.Error.Value.Message);
            }

            appointments = await repository.ListForPatientAsync(tenantId, patientId.Value, fromUtc, toUtc, cancellationToken);
        }
        else if (context.IsProfessionalScoped())
        {
            if (context.ActiveMembership?.ProfessionalId is not Guid professionalId)
            {
                return Result<IReadOnlyCollection<AppointmentReadModel>>.Fail("role_forbidden", "The active professional is not linked to a professional record.");
            }

            appointments = await repository.ListForProfessionalAsync(tenantId, professionalId, fromUtc, toUtc, cancellationToken);
        }
        else
        {
            appointments = await repository.ListAsync(tenantId, fromUtc, toUtc, cancellationToken);
        }

        return Result<IReadOnlyCollection<AppointmentReadModel>>.Ok(
            appointments.Select(appointment => new AppointmentReadModel(
                appointment.Id.Value,
                appointment.TenantId,
                appointment.PatientId,
                appointment.ProfessionalId,
                appointment.LocationId,
                appointment.StartsAtUtc,
                appointment.EndsAtUtc,
                appointment.Status,
                appointment.Notes)).ToArray());
    }
}

public sealed class ListAvailableSlotsHandler(
    IAppointmentRepository repository,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<ListAvailableSlotsQuery, Result<IReadOnlyCollection<AvailableSlotReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<AvailableSlotReadModel>>> Handle(
        ListAvailableSlotsQuery request,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return Result<IReadOnlyCollection<AvailableSlotReadModel>>.Validation(validationErrors);
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<IReadOnlyCollection<AvailableSlotReadModel>>.Fail("tenant_required", "Tenant context is required.");
        }

        var tenantId = context.GetRequiredTenantId();
        if (context.IsProfessionalScoped() &&
            context.ActiveMembership?.ProfessionalId != request.ProfessionalId)
        {
            return Result<IReadOnlyCollection<AvailableSlotReadModel>>.Fail("role_forbidden", "The active professional can only inspect their own schedule.");
        }

        if (!context.HasPermission(ClinicPermission.ManageSchedule) &&
            !context.HasPermission(ClinicPermission.ReadOwnClinicalSchedule))
        {
            return Result<IReadOnlyCollection<AvailableSlotReadModel>>.Fail("role_forbidden", "The active clinic role cannot inspect schedule slots.");
        }

        if (!await repository.ProfessionalExistsAsync(tenantId, request.ProfessionalId, cancellationToken))
        {
            return Result<IReadOnlyCollection<AvailableSlotReadModel>>.Fail("professional_not_found", "Professional was not found for the active tenant.");
        }

        if (request.LocationId is not null && !await repository.LocationExistsAsync(tenantId, request.LocationId.Value, cancellationToken))
        {
            return Result<IReadOnlyCollection<AvailableSlotReadModel>>.Fail("location_not_found", "Location was not found for the active tenant.");
        }

        var fromUtc = request.FromUtc ?? new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var toUtc = request.ToUtc ?? fromUtc.AddDays(1);
        var slotDuration = TimeSpan.FromMinutes(request.SlotMinutes);
        var busyWindows = await repository.ListBusyWindowsAsync(
            tenantId,
            request.ProfessionalId,
            request.LocationId,
            fromUtc,
            toUtc,
            cancellationToken);

        var slots = SlotAvailabilityPlanner.Calculate(fromUtc, toUtc, slotDuration, busyWindows)
            .Select(window => new AvailableSlotReadModel(
                request.ProfessionalId,
                request.LocationId,
                window.StartsAtUtc,
                window.EndsAtUtc))
            .ToArray();

        return Result<IReadOnlyCollection<AvailableSlotReadModel>>.Ok(slots);
    }

    private static Dictionary<string, string[]> Validate(ListAvailableSlotsQuery request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.ProfessionalId == Guid.Empty)
        {
            errors[nameof(ListAvailableSlotsQuery.ProfessionalId)] = ["Professional id is required."];
        }

        if (request.SlotMinutes is < 5 or > 180)
        {
            errors[nameof(ListAvailableSlotsQuery.SlotMinutes)] = ["Slot duration must be between 5 and 180 minutes."];
        }

        if (request.FromUtc is not null && request.ToUtc is not null)
        {
            if (request.ToUtc <= request.FromUtc)
            {
                errors[nameof(ListAvailableSlotsQuery.ToUtc)] = ["The slot search window end must be after start."];
            }

            if (request.ToUtc > request.FromUtc.Value.AddDays(31))
            {
                errors[nameof(ListAvailableSlotsQuery.ToUtc)] = ["The slot search window cannot exceed 31 days."];
            }
        }

        return errors;
    }
}

public sealed class CancelAppointmentHandler(
    IAppointmentRepository repository,
    IUnitOfWork unitOfWork,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<CancelAppointmentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        if (request.AppointmentId == Guid.Empty)
        {
            return Result<bool>.Validation(new Dictionary<string, string[]>
            {
                [nameof(CancelAppointmentCommand.AppointmentId)] = ["Appointment id is required."]
            });
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<bool>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageSchedule))
        {
            return Result<bool>.Fail("role_forbidden", "The active clinic role cannot cancel appointments.");
        }

        var cancelled = await repository.CancelAsync(context.GetRequiredTenantId(), request.AppointmentId, cancellationToken);
        if (!cancelled)
        {
            return Result<bool>.Fail("appointment_not_found", "Appointment was not found for the active tenant.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true);
    }
}

public sealed class RescheduleAppointmentHandler(
    IAppointmentRepository repository,
    IUnitOfWork unitOfWork,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<RescheduleAppointmentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return Result<bool>.Validation(validationErrors);
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<bool>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageSchedule))
        {
            return Result<bool>.Fail("role_forbidden", "The active clinic role cannot reschedule appointments.");
        }

        var tenantId = context.GetRequiredTenantId();
        if (!await repository.ProfessionalExistsAsync(tenantId, request.ProfessionalId, cancellationToken))
        {
            return Result<bool>.Fail("professional_not_found", "Professional was not found for the active tenant.");
        }

        if (request.LocationId is not null && !await repository.LocationExistsAsync(tenantId, request.LocationId.Value, cancellationToken))
        {
            return Result<bool>.Fail("location_not_found", "Location was not found for the active tenant.");
        }

        if (await repository.HasProfessionalConflictAsync(tenantId, request.ProfessionalId, request.StartsAtUtc, request.EndsAtUtc, request.AppointmentId, cancellationToken))
        {
            return Result<bool>.Fail("appointment_conflict", "The professional already has an appointment in this time window.");
        }

        var updated = await repository.RescheduleAsync(
            tenantId,
            request.AppointmentId,
            request.ProfessionalId,
            request.LocationId,
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.Notes,
            cancellationToken);

        if (!updated)
        {
            return Result<bool>.Fail("appointment_not_found", "Appointment was not found for the active tenant.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true);
    }

    private static Dictionary<string, string[]> Validate(RescheduleAppointmentCommand request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.AppointmentId == Guid.Empty)
        {
            errors[nameof(RescheduleAppointmentCommand.AppointmentId)] = ["Appointment id is required."];
        }

        if (request.ProfessionalId == Guid.Empty)
        {
            errors[nameof(RescheduleAppointmentCommand.ProfessionalId)] = ["Professional id is required."];
        }

        if (request.EndsAtUtc <= request.StartsAtUtc)
        {
            errors[nameof(RescheduleAppointmentCommand.EndsAtUtc)] = ["Appointment end must be after start."];
        }

        if (request.StartsAtUtc < DateTimeOffset.UtcNow.AddYears(-1))
        {
            errors[nameof(RescheduleAppointmentCommand.StartsAtUtc)] = ["Appointment start is invalid."];
        }

        return errors;
    }
}

public sealed class ListProfessionalsHandler(
    IAppointmentRepository repository,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<ListProfessionalsQuery, Result<IReadOnlyCollection<ProfessionalReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<ProfessionalReadModel>>> Handle(
        ListProfessionalsQuery request,
        CancellationToken cancellationToken)
    {
        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<IReadOnlyCollection<ProfessionalReadModel>>.Fail("tenant_required", "Tenant context is required.");
        }

        var professionals = await repository.ListProfessionalsAsync(context.GetRequiredTenantId(), cancellationToken);
        return Result<IReadOnlyCollection<ProfessionalReadModel>>.Ok(professionals);
    }
}

public sealed class ListSpecialtiesHandler(
    IAppointmentRepository repository,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<ListSpecialtiesQuery, Result<IReadOnlyCollection<SpecialtyReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<SpecialtyReadModel>>> Handle(ListSpecialtiesQuery request, CancellationToken cancellationToken)
    {
        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<IReadOnlyCollection<SpecialtyReadModel>>.Fail("tenant_required", "Tenant context is required.");
        }

        return Result<IReadOnlyCollection<SpecialtyReadModel>>.Ok(
            await repository.ListSpecialtiesAsync(context.GetRequiredTenantId(), cancellationToken));
    }
}

public sealed class ListLocationsHandler(
    IAppointmentRepository repository,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<ListLocationsQuery, Result<IReadOnlyCollection<LocationReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<LocationReadModel>>> Handle(ListLocationsQuery request, CancellationToken cancellationToken)
    {
        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<IReadOnlyCollection<LocationReadModel>>.Fail("tenant_required", "Tenant context is required.");
        }

        return Result<IReadOnlyCollection<LocationReadModel>>.Ok(
            await repository.ListLocationsAsync(context.GetRequiredTenantId(), cancellationToken));
    }
}

public sealed class CreateSpecialtyHandler(
    IAppointmentRepository repository,
    IUnitOfWork unitOfWork,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<CreateSpecialtyCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSpecialtyCommand request, CancellationToken cancellationToken)
    {
        var errors = SchedulingValidation.ValidateName(request.Name, nameof(CreateSpecialtyCommand.Name));
        if (errors.Count > 0)
        {
            return Result<Guid>.Validation(errors);
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<Guid>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageProfessionals))
        {
            return Result<Guid>.Fail("role_forbidden", "The active clinic role cannot create specialties.");
        }

        var id = await repository.AddSpecialtyAsync(context.GetRequiredTenantId(), request.Name.Trim(), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Ok(id);
    }
}

public sealed class CreateLocationHandler(
    IAppointmentRepository repository,
    IUnitOfWork unitOfWork,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<CreateLocationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        var errors = SchedulingValidation.ValidateName(request.Name, nameof(CreateLocationCommand.Name));
        if (string.IsNullOrWhiteSpace(request.Timezone))
        {
            errors[nameof(CreateLocationCommand.Timezone)] = ["Timezone is required."];
        }

        if (errors.Count > 0)
        {
            return Result<Guid>.Validation(errors);
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<Guid>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageProfessionals))
        {
            return Result<Guid>.Fail("role_forbidden", "The active clinic role cannot create locations.");
        }

        var id = await repository.AddLocationAsync(context.GetRequiredTenantId(), request.Name.Trim(), request.Timezone.Trim(), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Ok(id);
    }
}

public sealed class CreateProfessionalHandler(
    IAppointmentRepository repository,
    IUnitOfWork unitOfWork,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<CreateProfessionalCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProfessionalCommand request, CancellationToken cancellationToken)
    {
        var errors = SchedulingValidation.ValidateName(request.FullName, nameof(CreateProfessionalCommand.FullName));
        if (request.SpecialtyId == Guid.Empty)
        {
            errors[nameof(CreateProfessionalCommand.SpecialtyId)] = ["Specialty is required."];
        }

        if (errors.Count > 0)
        {
            return Result<Guid>.Validation(errors);
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<Guid>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageProfessionals))
        {
            return Result<Guid>.Fail("role_forbidden", "The active clinic role cannot create professionals.");
        }

        var tenantId = context.GetRequiredTenantId();
        if (!await repository.SpecialtyExistsAsync(tenantId, request.SpecialtyId, cancellationToken))
        {
            return Result<Guid>.Fail("specialty_not_found", "Specialty was not found for the active tenant.");
        }

        var id = await repository.AddProfessionalAsync(tenantId, request.FullName.Trim(), request.SpecialtyId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Ok(id);
    }
}

public sealed class RegisterManualWhatsappIntentHandler(
    IAppointmentRepository repository,
    IUnitOfWork unitOfWork,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<RegisterManualWhatsappIntentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterManualWhatsappIntentCommand request, CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return Result<Guid>.Validation(errors);
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<Guid>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageSchedule))
        {
            return Result<Guid>.Fail("role_forbidden", "The active clinic role cannot register WhatsApp intents.");
        }

        var tenantId = context.GetRequiredTenantId();
        if (!await repository.AppointmentExistsAsync(tenantId, request.AppointmentId, cancellationToken))
        {
            return Result<Guid>.Fail("appointment_not_found", "Appointment was not found for the active tenant.");
        }

        var messageId = await repository.AddManualWhatsappIntentAsync(
            tenantId,
            request.AppointmentId,
            request.RecipientPhone.Trim(),
            request.MessageText.Trim(),
            context.SubjectId,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Ok(messageId);
    }

    private static Dictionary<string, string[]> Validate(RegisterManualWhatsappIntentCommand request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.AppointmentId == Guid.Empty)
        {
            errors[nameof(RegisterManualWhatsappIntentCommand.AppointmentId)] = ["Appointment id is required."];
        }

        if (string.IsNullOrWhiteSpace(request.RecipientPhone))
        {
            errors[nameof(RegisterManualWhatsappIntentCommand.RecipientPhone)] = ["Recipient phone is required."];
        }
        else if (request.RecipientPhone.Trim().Length > 32)
        {
            errors[nameof(RegisterManualWhatsappIntentCommand.RecipientPhone)] = ["Recipient phone must have at most 32 characters."];
        }

        if (string.IsNullOrWhiteSpace(request.MessageText))
        {
            errors[nameof(RegisterManualWhatsappIntentCommand.MessageText)] = ["Message text is required."];
        }
        else if (request.MessageText.Trim().Length > 600)
        {
            errors[nameof(RegisterManualWhatsappIntentCommand.MessageText)] = ["Message text must have at most 600 characters."];
        }

        return errors;
    }
}

public sealed class MarkManualWhatsappSentHandler(
    IAppointmentRepository repository,
    IUnitOfWork unitOfWork,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<MarkManualWhatsappSentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(MarkManualWhatsappSentCommand request, CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return Result<bool>.Validation(errors);
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<bool>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageSchedule))
        {
            return Result<bool>.Fail("role_forbidden", "The active clinic role cannot update WhatsApp messages.");
        }

        var tenantId = context.GetRequiredTenantId();
        if (!await repository.AppointmentExistsAsync(tenantId, request.AppointmentId, cancellationToken))
        {
            return Result<bool>.Fail("appointment_not_found", "Appointment was not found for the active tenant.");
        }

        var updated = await repository.MarkManualWhatsappSentAsync(
            tenantId,
            request.AppointmentId,
            request.MessageId,
            string.IsNullOrWhiteSpace(request.Provider) ? null : request.Provider.Trim(),
            string.IsNullOrWhiteSpace(request.ProviderMessageId) ? null : request.ProviderMessageId.Trim(),
            DateTimeOffset.UtcNow,
            cancellationToken);

        if (!updated)
        {
            return Result<bool>.Fail("whatsapp_message_not_found", "WhatsApp message intent was not found for this appointment.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true);
    }

    private static Dictionary<string, string[]> Validate(MarkManualWhatsappSentCommand request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.AppointmentId == Guid.Empty)
        {
            errors[nameof(MarkManualWhatsappSentCommand.AppointmentId)] = ["Appointment id is required."];
        }

        if (request.MessageId == Guid.Empty)
        {
            errors[nameof(MarkManualWhatsappSentCommand.MessageId)] = ["Message id is required."];
        }

        if (!string.IsNullOrWhiteSpace(request.Provider) && request.Provider.Trim().Length > 80)
        {
            errors[nameof(MarkManualWhatsappSentCommand.Provider)] = ["Provider must have at most 80 characters."];
        }

        if (!string.IsNullOrWhiteSpace(request.ProviderMessageId) && request.ProviderMessageId.Trim().Length > 160)
        {
            errors[nameof(MarkManualWhatsappSentCommand.ProviderMessageId)] = ["Provider message id must have at most 160 characters."];
        }

        return errors;
    }
}

public sealed class ListManualWhatsappMessagesHandler(
    IAppointmentRepository repository,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<ListManualWhatsappMessagesQuery, Result<IReadOnlyCollection<ManualWhatsappMessageReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<ManualWhatsappMessageReadModel>>> Handle(
        ListManualWhatsappMessagesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.AppointmentId == Guid.Empty)
        {
            return Result<IReadOnlyCollection<ManualWhatsappMessageReadModel>>.Validation(new Dictionary<string, string[]>
            {
                [nameof(ListManualWhatsappMessagesQuery.AppointmentId)] = ["Appointment id is required."]
            });
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<IReadOnlyCollection<ManualWhatsappMessageReadModel>>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageSchedule))
        {
            return Result<IReadOnlyCollection<ManualWhatsappMessageReadModel>>.Fail("role_forbidden", "The active clinic role cannot list WhatsApp messages.");
        }

        var tenantId = context.GetRequiredTenantId();
        if (!await repository.AppointmentExistsAsync(tenantId, request.AppointmentId, cancellationToken))
        {
            return Result<IReadOnlyCollection<ManualWhatsappMessageReadModel>>.Fail("appointment_not_found", "Appointment was not found for the active tenant.");
        }

        var messages = await repository.ListManualWhatsappMessagesAsync(tenantId, request.AppointmentId, cancellationToken);
        return Result<IReadOnlyCollection<ManualWhatsappMessageReadModel>>.Ok(messages);
    }
}

public sealed class CreateChargeHandler(
    IAppointmentRepository repository,
    IUnitOfWork unitOfWork,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<CreateChargeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateChargeCommand request, CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return Result<Guid>.Validation(errors);
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<Guid>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageBilling))
        {
            return Result<Guid>.Fail("role_forbidden", "The active clinic role cannot create charges.");
        }

        var tenantId = context.GetRequiredTenantId();
        if (!await repository.AppointmentExistsAsync(tenantId, request.AppointmentId, cancellationToken))
        {
            return Result<Guid>.Fail("appointment_not_found", "Appointment was not found for the active tenant.");
        }

        if (!await repository.PatientExistsAsync(tenantId, request.PatientId, cancellationToken))
        {
            return Result<Guid>.Fail("patient_not_found", "Patient was not found for the active tenant.");
        }

        var chargeId = await repository.AddChargeAsync(
            tenantId,
            request.AppointmentId,
            request.PatientId,
            request.PayerType.Trim(),
            request.Description.Trim(),
            request.Amount,
            request.DueDate,
            context.SubjectId,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Ok(chargeId);
    }

    private static Dictionary<string, string[]> Validate(CreateChargeCommand request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.AppointmentId == Guid.Empty)
        {
            errors[nameof(CreateChargeCommand.AppointmentId)] = ["Appointment id is required."];
        }

        if (request.PatientId == Guid.Empty)
        {
            errors[nameof(CreateChargeCommand.PatientId)] = ["Patient id is required."];
        }

        if (request.Amount <= 0)
        {
            errors[nameof(CreateChargeCommand.Amount)] = ["Amount must be greater than zero."];
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            errors[nameof(CreateChargeCommand.Description)] = ["Description is required."];
        }

        var normalizedPayer = request.PayerType?.Trim().ToLowerInvariant();
        if (normalizedPayer is not ("insurance" or "private"))
        {
            errors[nameof(CreateChargeCommand.PayerType)] = ["Payer type must be insurance or private."];
        }

        return errors;
    }
}

public sealed class MarkChargeAsPaidHandler(
    IAppointmentRepository repository,
    IUnitOfWork unitOfWork,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<MarkChargeAsPaidCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(MarkChargeAsPaidCommand request, CancellationToken cancellationToken)
    {
        if (request.ChargeId == Guid.Empty)
        {
            return Result<bool>.Validation(new Dictionary<string, string[]>
            {
                [nameof(MarkChargeAsPaidCommand.ChargeId)] = ["Charge id is required."]
            });
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<bool>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageBilling))
        {
            return Result<bool>.Fail("role_forbidden", "The active clinic role cannot update charges.");
        }

        var updated = await repository.MarkChargeAsPaidAsync(
            context.GetRequiredTenantId(),
            request.ChargeId,
            DateTimeOffset.UtcNow,
            cancellationToken);

        if (!updated)
        {
            return Result<bool>.Fail("charge_not_found", "Charge was not found for the active tenant.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true);
    }
}

public sealed class ListChargesHandler(
    IAppointmentRepository repository,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<ListChargesQuery, Result<IReadOnlyCollection<ChargeReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<ChargeReadModel>>> Handle(ListChargesQuery request, CancellationToken cancellationToken)
    {
        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<IReadOnlyCollection<ChargeReadModel>>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageBilling))
        {
            return Result<IReadOnlyCollection<ChargeReadModel>>.Fail("role_forbidden", "The active clinic role cannot list charges.");
        }

        var normalizedStatus = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim().ToLowerInvariant();
        if (normalizedStatus is not null and not ("open" or "paid" or "batched" or "overdue"))
        {
            return Result<IReadOnlyCollection<ChargeReadModel>>.Validation(new Dictionary<string, string[]>
            {
                [nameof(ListChargesQuery.Status)] = ["Status must be open, paid, batched or overdue."]
            });
        }

        var charges = await repository.ListChargesAsync(context.GetRequiredTenantId(), normalizedStatus, cancellationToken);
        return Result<IReadOnlyCollection<ChargeReadModel>>.Ok(charges);
    }
}

public sealed class CloseBillingBatchHandler(
    IAppointmentRepository repository,
    IUnitOfWork unitOfWork,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<CloseBillingBatchCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CloseBillingBatchCommand request, CancellationToken cancellationToken)
    {
        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<Guid>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageBilling))
        {
            return Result<Guid>.Fail("role_forbidden", "The active clinic role cannot close billing batches.");
        }

        var batchId = await repository.CloseBatchAsync(
            context.GetRequiredTenantId(),
            request.CompetenceMonth,
            context.SubjectId,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Ok(batchId);
    }
}

public sealed class ListBillingBatchesHandler(
    IAppointmentRepository repository,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<ListBillingBatchesQuery, Result<IReadOnlyCollection<BillingBatchReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<BillingBatchReadModel>>> Handle(ListBillingBatchesQuery request, CancellationToken cancellationToken)
    {
        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<IReadOnlyCollection<BillingBatchReadModel>>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManageBilling))
        {
            return Result<IReadOnlyCollection<BillingBatchReadModel>>.Fail("role_forbidden", "The active clinic role cannot list billing batches.");
        }

        var batches = await repository.ListBatchesAsync(context.GetRequiredTenantId(), cancellationToken);
        return Result<IReadOnlyCollection<BillingBatchReadModel>>.Ok(batches);
    }
}

internal static class SchedulingValidation
{
    public static Dictionary<string, string[]> ValidateName(string value, string memberName)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[memberName] = ["Name is required."];
        }

        return errors;
    }
}

internal readonly record struct AppointmentPatientIdResolution(Guid Value, (string Code, string Message)? Error);

internal static class AppointmentPatientIdentityGuards
{
    public static async Task<AppointmentPatientIdResolution> ResolvePatientIdAsync(
        ResolvedRequestContext context,
        IAppointmentRepository repository,
        Guid tenantId,
        Guid requestedPatientId,
        CancellationToken cancellationToken)
    {
        if (!context.IsPatientScoped())
        {
            return new AppointmentPatientIdResolution(requestedPatientId, null);
        }

        var ownPatientId = await repository.GetPatientIdForSubjectAsync(tenantId, context.SubjectId, cancellationToken);
        return ownPatientId is Guid patientId
            ? new AppointmentPatientIdResolution(patientId, null)
            : new AppointmentPatientIdResolution(Guid.Empty, ("patient_identity_not_found", "The authenticated patient is not linked to a patient record."));
    }
}

internal static class SlotAvailabilityPlanner
{
    public static IReadOnlyCollection<BusyWindowReadModel> Calculate(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        TimeSpan slotDuration,
        IReadOnlyCollection<BusyWindowReadModel> busyWindows)
    {
        if (toUtc <= fromUtc || slotDuration <= TimeSpan.Zero)
        {
            return [];
        }

        var mergedBusyWindows = MergeBusyWindows(busyWindows, fromUtc, toUtc);
        var slots = new List<BusyWindowReadModel>();
        var cursor = fromUtc;
        var windowIndex = 0;

        while (cursor + slotDuration <= toUtc)
        {
            var slotEnd = cursor + slotDuration;

            while (windowIndex < mergedBusyWindows.Count && mergedBusyWindows[windowIndex].EndsAtUtc <= cursor)
            {
                windowIndex++;
            }

            var overlaps = windowIndex < mergedBusyWindows.Count
                           && mergedBusyWindows[windowIndex].StartsAtUtc < slotEnd
                           && mergedBusyWindows[windowIndex].EndsAtUtc > cursor;

            if (!overlaps)
            {
                slots.Add(new BusyWindowReadModel(cursor, slotEnd));
            }

            cursor = slotEnd;
        }

        return slots;
    }

    private static List<BusyWindowReadModel> MergeBusyWindows(
        IReadOnlyCollection<BusyWindowReadModel> busyWindows,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc)
    {
        var ordered = busyWindows
            .Where(window => window.EndsAtUtc > fromUtc && window.StartsAtUtc < toUtc)
            .Select(window => new BusyWindowReadModel(
                window.StartsAtUtc < fromUtc ? fromUtc : window.StartsAtUtc,
                window.EndsAtUtc > toUtc ? toUtc : window.EndsAtUtc))
            .OrderBy(window => window.StartsAtUtc)
            .ToArray();

        if (ordered.Length == 0)
        {
            return [];
        }

        var merged = new List<BusyWindowReadModel> { ordered[0] };
        for (var i = 1; i < ordered.Length; i++)
        {
            var current = ordered[i];
            var previous = merged[^1];

            if (current.StartsAtUtc <= previous.EndsAtUtc)
            {
                merged[^1] = new BusyWindowReadModel(
                    previous.StartsAtUtc,
                    current.EndsAtUtc > previous.EndsAtUtc ? current.EndsAtUtc : previous.EndsAtUtc);
                continue;
            }

            merged.Add(current);
        }

        return merged;
    }
}

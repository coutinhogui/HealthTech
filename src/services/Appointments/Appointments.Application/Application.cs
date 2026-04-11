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
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string Status,
    string? Notes);

public record CreateAppointmentCommand(
    Guid PatientId,
    Guid ProfessionalId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Notes) : IRequest<Result<Guid>>;

public record ListAppointmentsQuery(DateTimeOffset? FromUtc = null, DateTimeOffset? ToUtc = null) : IRequest<Result<IReadOnlyCollection<AppointmentReadModel>>>;

public interface IAppointmentRepository
{
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Appointment>> ListAsync(Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken);
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
        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<Guid>.Fail("Tenant context is required.");
        }

        using var activity = HealthTechTelemetry.ActivitySource.StartActivity("appointments.create");
        activity?.SetTag("tenant.id", context.GetRequiredTenantId());
        activity?.SetTag("professional.id", request.ProfessionalId);

        var appointment = new Appointment(
            AppointmentId.New(),
            context.GetRequiredTenantId(),
            request.PatientId,
            request.ProfessionalId,
            request.StartsAtUtc,
            request.EndsAtUtc,
            "scheduled",
            request.Notes);

        await repository.AddAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        AppointmentsBooked.Add(1, new KeyValuePair<string, object?>("tenant.id", context.GetRequiredTenantId()));

        return Result<Guid>.Ok(appointment.Id.Value);
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
            return Result<IReadOnlyCollection<AppointmentReadModel>>.Fail("Tenant context is required.");
        }

        var fromUtc = request.FromUtc ?? new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var toUtc = request.ToUtc ?? fromUtc.AddDays(7);

        var appointments = await repository.ListAsync(context.GetRequiredTenantId(), fromUtc, toUtc, cancellationToken);
        return Result<IReadOnlyCollection<AppointmentReadModel>>.Ok(
            appointments.Select(appointment => new AppointmentReadModel(
                appointment.Id.Value,
                appointment.TenantId,
                appointment.PatientId,
                appointment.ProfessionalId,
                appointment.StartsAtUtc,
                appointment.EndsAtUtc,
                appointment.Status,
                appointment.Notes)).ToArray());
    }
}

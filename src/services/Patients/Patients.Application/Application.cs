using System.Diagnostics.Metrics;
using HealthTech.BuildingBlocks.Abstractions;
using HealthTech.BuildingBlocks.SharedKernel;
using HealthTech.Patients.Domain;
using MediatR;

namespace HealthTech.Patients.Application;

public sealed record PatientReadModel(Guid Id, Guid TenantId, string FullName, string Document, DateOnly BirthDate);

public record RegisterPatientCommand(string FullName, string Document, DateOnly BirthDate) : IRequest<Result<Guid>>;

public record GetPatientByIdQuery(Guid PatientId) : IRequest<Result<PatientReadModel>>;

public record ListPatientsQuery(int Skip = 0, int Take = 50) : IRequest<Result<IReadOnlyCollection<PatientReadModel>>>;

public interface IPatientRepository
{
    Task AddAsync(Patient entity, CancellationToken ct);
    Task<Patient?> GetAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<Patient>> ListAsync(Guid tenantId, int skip, int take, CancellationToken ct);
}

public sealed class RegisterPatientHandler(
    IPatientRepository repo,
    IUnitOfWork uow,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<RegisterPatientCommand, Result<Guid>>
{
    private static readonly Counter<long> PatientsRegistered =
        HealthTechTelemetry.Meter.CreateCounter<long>("healthtech.patients.registered");

    public async Task<Result<Guid>> Handle(RegisterPatientCommand request, CancellationToken ct)
    {
        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<Guid>.Fail("Tenant context is required. Set the X-Tenant-Id header for users with multiple memberships.");
        }

        using var activity = HealthTechTelemetry.ActivitySource.StartActivity("patients.register");
        activity?.SetTag("tenant.id", context.GetRequiredTenantId());
        activity?.SetTag("patient.document", request.Document);

        var entity = new Patient(PatientId.New(), context.GetRequiredTenantId(), request.FullName, request.Document, request.BirthDate);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        PatientsRegistered.Add(1, new KeyValuePair<string, object?>("tenant.id", context.GetRequiredTenantId()));

        return Result<Guid>.Ok(entity.Id.Value);
    }
}

public sealed class GetPatientByIdHandler(
    IPatientRepository repo,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<GetPatientByIdQuery, Result<PatientReadModel>>
{
    public async Task<Result<PatientReadModel>> Handle(GetPatientByIdQuery request, CancellationToken ct)
    {
        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<PatientReadModel>.Fail("Tenant context is required.");
        }

        var patient = await repo.GetAsync(context.GetRequiredTenantId(), request.PatientId, ct);
        return patient is null
            ? Result<PatientReadModel>.Fail("Patient not found.")
            : Result<PatientReadModel>.Ok(new PatientReadModel(
                patient.Id.Value,
                patient.TenantId,
                patient.FullName,
                patient.Document,
                patient.BirthDate));
    }
}

public sealed class ListPatientsHandler(
    IPatientRepository repo,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<ListPatientsQuery, Result<IReadOnlyCollection<PatientReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<PatientReadModel>>> Handle(ListPatientsQuery request, CancellationToken ct)
    {
        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<IReadOnlyCollection<PatientReadModel>>.Fail("Tenant context is required.");
        }

        var patients = await repo.ListAsync(context.GetRequiredTenantId(), request.Skip, request.Take, ct);
        return Result<IReadOnlyCollection<PatientReadModel>>.Ok(
            patients.Select(patient => new PatientReadModel(
                patient.Id.Value,
                patient.TenantId,
                patient.FullName,
                patient.Document,
                patient.BirthDate)).ToArray());
    }
}

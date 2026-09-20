using System.Diagnostics.Metrics;
using HealthTech.BuildingBlocks.Abstractions;
using HealthTech.BuildingBlocks.SharedKernel;
using HealthTech.Patients.Domain;
using MediatR;

namespace HealthTech.Patients.Application;

public sealed record PatientReadModel(Guid Id, Guid TenantId, string FullName, string Document, DateOnly BirthDate);
public sealed record InsurancePlanReadModel(Guid Id, Guid TenantId, string PayerName, string PlanName, bool Active);
public sealed record PatientPlanReadModel(Guid PatientId, Guid PlanId, string PayerName, string PlanName, bool Active, DateTimeOffset LinkedAtUtc);
public sealed record PatientEvolutionReadModel(
    Guid Id,
    Guid TenantId,
    Guid PatientId,
    DateTimeOffset EncounteredAtUtc,
    string Subjective,
    string Objective,
    string Assessment,
    string Plan,
    string? CreatedBySubject,
    DateTimeOffset CreatedAtUtc);

public record RegisterPatientCommand(string FullName, string Document, DateOnly BirthDate) : IRequest<Result<Guid>>;

public record GetPatientByIdQuery(Guid PatientId) : IRequest<Result<PatientReadModel>>;

public record ListPatientsQuery(int Skip = 0, int Take = 50) : IRequest<Result<IReadOnlyCollection<PatientReadModel>>>;
public record CreateInsurancePlanCommand(string PayerName, string PlanName) : IRequest<Result<Guid>>;
public record ListInsurancePlansQuery : IRequest<Result<IReadOnlyCollection<InsurancePlanReadModel>>>;
public record AssignPatientPlanCommand(Guid PatientId, Guid PlanId) : IRequest<Result<bool>>;
public record ListPatientPlansQuery(Guid PatientId) : IRequest<Result<IReadOnlyCollection<PatientPlanReadModel>>>;
public record CreatePatientEvolutionCommand(
    Guid PatientId,
    DateTimeOffset? EncounteredAtUtc,
    string Subjective,
    string Objective,
    string Assessment,
    string Plan) : IRequest<Result<Guid>>;
public record ListPatientEvolutionsQuery(Guid PatientId, int Skip = 0, int Take = 50) : IRequest<Result<IReadOnlyCollection<PatientEvolutionReadModel>>>;

public interface IPatientRepository
{
    Task AddAsync(Patient entity, CancellationToken ct);
    Task<Patient?> GetAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<Patient>> ListAsync(Guid tenantId, int skip, int take, CancellationToken ct);
    Task<IReadOnlyCollection<Patient>> ListForProfessionalAsync(Guid tenantId, Guid professionalId, int skip, int take, CancellationToken ct);
    Task<bool> PatientHasAppointmentWithProfessionalAsync(Guid tenantId, Guid patientId, Guid professionalId, CancellationToken ct);
    Task<Guid?> GetPatientIdForSubjectAsync(Guid tenantId, string subjectId, CancellationToken ct);
    Task<Guid> AddInsurancePlanAsync(Guid tenantId, string payerName, string planName, CancellationToken ct);
    Task<IReadOnlyCollection<InsurancePlanReadModel>> ListInsurancePlansAsync(Guid tenantId, CancellationToken ct);
    Task<bool> PlanExistsAsync(Guid tenantId, Guid planId, CancellationToken ct);
    Task UpsertPatientPlanAsync(Guid tenantId, Guid patientId, Guid planId, CancellationToken ct);
    Task<IReadOnlyCollection<PatientPlanReadModel>> ListPatientPlansAsync(Guid tenantId, Guid patientId, CancellationToken ct);
    Task<Guid> AddPatientEvolutionAsync(
        Guid tenantId,
        Guid patientId,
        DateTimeOffset encounteredAtUtc,
        string subjective,
        string objective,
        string assessment,
        string plan,
        string? createdBySubject,
        CancellationToken ct);
    Task<IReadOnlyCollection<PatientEvolutionReadModel>> ListPatientEvolutionsAsync(Guid tenantId, Guid patientId, int skip, int take, CancellationToken ct);
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
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return Result<Guid>.Validation(validationErrors);
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<Guid>.Fail("tenant_required", "Tenant context is required. Set the X-Tenant-Id header for users with multiple memberships.");
        }

        if (!context.HasPermission(ClinicPermission.ManagePatients))
        {
            return Result<Guid>.Fail("role_forbidden", "The active clinic role cannot register patients.");
        }

        using var activity = HealthTechTelemetry.ActivitySource.StartActivity("patients.register");
        activity?.SetTag("tenant.id", context.GetRequiredTenantId());

        var entity = new Patient(PatientId.New(), context.GetRequiredTenantId(), request.FullName, request.Document, request.BirthDate);
        try
        {
            await repo.AddAsync(entity, ct);
        }
        catch (DuplicatePatientDocumentException)
        {
            return Result<Guid>.Fail("patient_document_conflict", "A patient with this document already exists for this tenant.");
        }

        await uow.SaveChangesAsync(ct);

        PatientsRegistered.Add(1, new KeyValuePair<string, object?>("tenant.id", context.GetRequiredTenantId()));

        return Result<Guid>.Ok(entity.Id.Value);
    }

    private static Dictionary<string, string[]> Validate(RegisterPatientCommand request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            errors[nameof(RegisterPatientCommand.FullName)] = ["Full name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Document))
        {
            errors[nameof(RegisterPatientCommand.Document)] = ["Document is required."];
        }

        if (request.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            errors[nameof(RegisterPatientCommand.BirthDate)] = ["Birth date cannot be in the future."];
        }

        return errors;
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
            return Result<PatientReadModel>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ReadPatients) &&
            !context.HasPermission(ClinicPermission.ReadOwnPatientProfile))
        {
            return Result<PatientReadModel>.Fail("role_forbidden", "The active clinic role cannot read patients.");
        }

        var tenantId = context.GetRequiredTenantId();
        var patientId = request.PatientId;
        if (context.IsPatientScoped())
        {
            var ownPatientId = await repo.GetPatientIdForSubjectAsync(tenantId, context.SubjectId, ct);
            if (ownPatientId is null)
            {
                return Result<PatientReadModel>.Fail("patient_identity_not_found", "The authenticated patient is not linked to a patient record.");
            }

            patientId = ownPatientId.Value;
        }

        if (context.IsProfessionalScoped() && !await PatientAuthorizationGuards.ProfessionalCanAccessPatientAsync(context, repo, tenantId, request.PatientId, ct))
        {
            return Result<PatientReadModel>.Fail("role_forbidden", "The active professional can only read patients from their own appointments.");
        }

        var patient = await repo.GetAsync(tenantId, patientId, ct);
        return patient is null
            ? Result<PatientReadModel>.Fail("patient_not_found", "Patient not found.")
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
            return Result<IReadOnlyCollection<PatientReadModel>>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ReadPatients) &&
            !context.HasPermission(ClinicPermission.ReadOwnPatientProfile))
        {
            return Result<IReadOnlyCollection<PatientReadModel>>.Fail("role_forbidden", "The active clinic role cannot list patients.");
        }

        var tenantId = context.GetRequiredTenantId();
        var patients = context.IsPatientScoped()
            ? await ListOwnPatientAsync(repo, context, tenantId, ct)
            : context.IsProfessionalScoped()
            ? context.ActiveMembership?.ProfessionalId is Guid professionalId
                ? await repo.ListForProfessionalAsync(tenantId, professionalId, request.Skip, request.Take, ct)
                : []
            : await repo.ListAsync(tenantId, request.Skip, request.Take, ct);
        return Result<IReadOnlyCollection<PatientReadModel>>.Ok(
            patients.Select(patient => new PatientReadModel(
                patient.Id.Value,
                patient.TenantId,
                patient.FullName,
                patient.Document,
                patient.BirthDate)).ToArray());
    }

    private static async Task<IReadOnlyCollection<Patient>> ListOwnPatientAsync(
        IPatientRepository repo,
        ResolvedRequestContext context,
        Guid tenantId,
        CancellationToken ct)
    {
        var ownPatientId = await repo.GetPatientIdForSubjectAsync(tenantId, context.SubjectId, ct);
        if (ownPatientId is null)
        {
            return [];
        }

        var ownPatient = await repo.GetAsync(tenantId, ownPatientId.Value, ct);
        return ownPatient is null ? [] : [ownPatient];
    }
}

public sealed class CreateInsurancePlanHandler(
    IPatientRepository repo,
    IUnitOfWork uow,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<CreateInsurancePlanCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateInsurancePlanCommand request, CancellationToken ct)
    {
        var errors = ValidateInsurancePlan(request);
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
            return Result<Guid>.Fail("role_forbidden", "The active clinic role cannot create insurance plans.");
        }

        try
        {
            var id = await repo.AddInsurancePlanAsync(context.GetRequiredTenantId(), request.PayerName.Trim(), request.PlanName.Trim(), ct);
            await uow.SaveChangesAsync(ct);
            return Result<Guid>.Ok(id);
        }
        catch (DuplicateInsurancePlanException)
        {
            return Result<Guid>.Fail("plan_conflict", "A plan with this payer and name already exists for this tenant.");
        }
    }

    private static Dictionary<string, string[]> ValidateInsurancePlan(CreateInsurancePlanCommand request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.PayerName))
        {
            errors[nameof(CreateInsurancePlanCommand.PayerName)] = ["Payer name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.PlanName))
        {
            errors[nameof(CreateInsurancePlanCommand.PlanName)] = ["Plan name is required."];
        }

        return errors;
    }
}

public sealed class ListInsurancePlansHandler(
    IPatientRepository repo,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<ListInsurancePlansQuery, Result<IReadOnlyCollection<InsurancePlanReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<InsurancePlanReadModel>>> Handle(ListInsurancePlansQuery request, CancellationToken ct)
    {
        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<IReadOnlyCollection<InsurancePlanReadModel>>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ReadBillingContext) &&
            !context.HasPermission(ClinicPermission.ManageBilling))
        {
            return Result<IReadOnlyCollection<InsurancePlanReadModel>>.Fail("role_forbidden", "The active clinic role cannot list insurance plans.");
        }

        var plans = await repo.ListInsurancePlansAsync(context.GetRequiredTenantId(), ct);
        return Result<IReadOnlyCollection<InsurancePlanReadModel>>.Ok(plans);
    }
}

public sealed class AssignPatientPlanHandler(
    IPatientRepository repo,
    IUnitOfWork uow,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<AssignPatientPlanCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(AssignPatientPlanCommand request, CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.PatientId == Guid.Empty)
        {
            errors[nameof(AssignPatientPlanCommand.PatientId)] = ["Patient id is required."];
        }

        if (request.PlanId == Guid.Empty)
        {
            errors[nameof(AssignPatientPlanCommand.PlanId)] = ["Plan id is required."];
        }

        if (errors.Count > 0)
        {
            return Result<bool>.Validation(errors);
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<bool>.Fail("tenant_required", "Tenant context is required.");
        }

        if (!context.HasPermission(ClinicPermission.ManagePatients) &&
            !context.HasPermission(ClinicPermission.ManageBilling))
        {
            return Result<bool>.Fail("role_forbidden", "The active clinic role cannot assign insurance plans.");
        }

        var tenantId = context.GetRequiredTenantId();
        if (await repo.GetAsync(tenantId, request.PatientId, ct) is null)
        {
            return Result<bool>.Fail("patient_not_found", "Patient not found.");
        }

        if (!await repo.PlanExistsAsync(tenantId, request.PlanId, ct))
        {
            return Result<bool>.Fail("plan_not_found", "Plan not found.");
        }

        await repo.UpsertPatientPlanAsync(tenantId, request.PatientId, request.PlanId, ct);
        await uow.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }
}

public sealed class ListPatientPlansHandler(
    IPatientRepository repo,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<ListPatientPlansQuery, Result<IReadOnlyCollection<PatientPlanReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<PatientPlanReadModel>>> Handle(ListPatientPlansQuery request, CancellationToken ct)
    {
        if (request.PatientId == Guid.Empty)
        {
            return Result<IReadOnlyCollection<PatientPlanReadModel>>.Validation(new Dictionary<string, string[]>
            {
                [nameof(ListPatientPlansQuery.PatientId)] = ["Patient id is required."]
            });
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<IReadOnlyCollection<PatientPlanReadModel>>.Fail("tenant_required", "Tenant context is required.");
        }

        var tenantId = context.GetRequiredTenantId();
        var patientId = await PatientIdentityGuards.ResolvePatientIdForReadAsync(context, repo, tenantId, request.PatientId, ct);
        if (patientId.Error is not null)
        {
            return Result<IReadOnlyCollection<PatientPlanReadModel>>.Fail(patientId.Error.Value.Code, patientId.Error.Value.Message);
        }

        if (context.IsProfessionalScoped() && !await PatientAuthorizationGuards.ProfessionalCanAccessPatientAsync(context, repo, tenantId, request.PatientId, ct))
        {
            return Result<IReadOnlyCollection<PatientPlanReadModel>>.Fail("role_forbidden", "The active professional can only read plans from their own patients.");
        }

        if (await repo.GetAsync(tenantId, patientId.Value, ct) is null)
        {
            return Result<IReadOnlyCollection<PatientPlanReadModel>>.Fail("patient_not_found", "Patient not found.");
        }

        var plans = await repo.ListPatientPlansAsync(tenantId, patientId.Value, ct);
        return Result<IReadOnlyCollection<PatientPlanReadModel>>.Ok(plans);
    }
}

public sealed class CreatePatientEvolutionHandler(
    IPatientRepository repo,
    IUnitOfWork uow,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<CreatePatientEvolutionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePatientEvolutionCommand request, CancellationToken ct)
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

        if (!context.HasPermission(ClinicPermission.WriteClinicalRecord))
        {
            return Result<Guid>.Fail("role_forbidden", "The active clinic role cannot create clinical evolutions.");
        }

        var tenantId = context.GetRequiredTenantId();
        var patientId = await PatientIdentityGuards.ResolvePatientIdForReadAsync(context, repo, tenantId, request.PatientId, ct);
        if (patientId.Error is not null)
        {
            return Result<Guid>.Fail(patientId.Error.Value.Code, patientId.Error.Value.Message);
        }

        if (context.IsProfessionalScoped() && !await PatientAuthorizationGuards.ProfessionalCanAccessPatientAsync(context, repo, tenantId, patientId.Value, ct))
        {
            return Result<Guid>.Fail("role_forbidden", "The active professional can only create evolutions for their own patients.");
        }

        if (await repo.GetAsync(tenantId, patientId.Value, ct) is null)
        {
            return Result<Guid>.Fail("patient_not_found", "Patient not found.");
        }

        var id = await repo.AddPatientEvolutionAsync(
            tenantId,
            patientId.Value,
            request.EncounteredAtUtc ?? DateTimeOffset.UtcNow,
            request.Subjective.Trim(),
            request.Objective.Trim(),
            request.Assessment.Trim(),
            request.Plan.Trim(),
            context.SubjectId,
            ct);

        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Ok(id);
    }

    private static Dictionary<string, string[]> Validate(CreatePatientEvolutionCommand request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.PatientId == Guid.Empty)
        {
            errors[nameof(CreatePatientEvolutionCommand.PatientId)] = ["Patient id is required."];
        }

        ValidateRequiredText(request.Subjective, nameof(CreatePatientEvolutionCommand.Subjective), errors);
        ValidateRequiredText(request.Objective, nameof(CreatePatientEvolutionCommand.Objective), errors);
        ValidateRequiredText(request.Assessment, nameof(CreatePatientEvolutionCommand.Assessment), errors);
        ValidateRequiredText(request.Plan, nameof(CreatePatientEvolutionCommand.Plan), errors);

        return errors;
    }

    private static void ValidateRequiredText(string value, string memberName, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[memberName] = [$"{memberName} is required."];
            return;
        }

        if (value.Trim().Length > 4000)
        {
            errors[memberName] = [$"{memberName} exceeds the limit of 4000 characters."];
        }
    }
}

public sealed class ListPatientEvolutionsHandler(
    IPatientRepository repo,
    IRequestContextAccessor requestContextAccessor) : IRequestHandler<ListPatientEvolutionsQuery, Result<IReadOnlyCollection<PatientEvolutionReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<PatientEvolutionReadModel>>> Handle(ListPatientEvolutionsQuery request, CancellationToken ct)
    {
        if (request.PatientId == Guid.Empty)
        {
            return Result<IReadOnlyCollection<PatientEvolutionReadModel>>.Validation(new Dictionary<string, string[]>
            {
                [nameof(ListPatientEvolutionsQuery.PatientId)] = ["Patient id is required."]
            });
        }

        var context = requestContextAccessor.Current;
        if (!context.HasActiveTenant)
        {
            return Result<IReadOnlyCollection<PatientEvolutionReadModel>>.Fail("tenant_required", "Tenant context is required.");
        }

        var tenantId = context.GetRequiredTenantId();
        var patientId = await PatientIdentityGuards.ResolvePatientIdForReadAsync(context, repo, tenantId, request.PatientId, ct);
        if (patientId.Error is not null)
        {
            return Result<IReadOnlyCollection<PatientEvolutionReadModel>>.Fail(patientId.Error.Value.Code, patientId.Error.Value.Message);
        }

        if (!context.HasPermission(ClinicPermission.ReadClinicalRecord))
        {
            if (!context.HasPermission(ClinicPermission.ReadOwnPatientProfile))
            {
                return Result<IReadOnlyCollection<PatientEvolutionReadModel>>.Fail("role_forbidden", "The active clinic role cannot read clinical evolutions.");
            }
        }

        if (context.IsProfessionalScoped() && !await PatientAuthorizationGuards.ProfessionalCanAccessPatientAsync(context, repo, tenantId, patientId.Value, ct))
        {
            return Result<IReadOnlyCollection<PatientEvolutionReadModel>>.Fail("role_forbidden", "The active professional can only read evolutions from their own patients.");
        }

        if (await repo.GetAsync(tenantId, patientId.Value, ct) is null)
        {
            return Result<IReadOnlyCollection<PatientEvolutionReadModel>>.Fail("patient_not_found", "Patient not found.");
        }

        var evolutions = await repo.ListPatientEvolutionsAsync(
            tenantId,
            patientId.Value,
            request.Skip,
            request.Take,
            ct);

        return Result<IReadOnlyCollection<PatientEvolutionReadModel>>.Ok(evolutions);
    }
}

internal static class PatientAuthorizationGuards
{
    public static async Task<bool> ProfessionalCanAccessPatientAsync(
        ResolvedRequestContext context,
        IPatientRepository repo,
        Guid tenantId,
        Guid patientId,
        CancellationToken ct)
        => context.ActiveMembership?.ProfessionalId is Guid professionalId &&
           await repo.PatientHasAppointmentWithProfessionalAsync(tenantId, patientId, professionalId, ct);
}

internal readonly record struct PatientIdResolution(Guid Value, (string Code, string Message)? Error);

internal static class PatientIdentityGuards
{
    public static async Task<PatientIdResolution> ResolvePatientIdForReadAsync(
        ResolvedRequestContext context,
        IPatientRepository repo,
        Guid tenantId,
        Guid requestedPatientId,
        CancellationToken ct)
    {
        if (!context.IsPatientScoped())
        {
            return new PatientIdResolution(requestedPatientId, null);
        }

        var ownPatientId = await repo.GetPatientIdForSubjectAsync(tenantId, context.SubjectId, ct);
        return ownPatientId is Guid patientId
            ? new PatientIdResolution(patientId, null)
            : new PatientIdResolution(Guid.Empty, ("patient_identity_not_found", "The authenticated patient is not linked to a patient record."));
    }
}

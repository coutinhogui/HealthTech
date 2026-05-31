using System.Net;
using System.Net.Http.Json;

namespace HealthTech.Frontends.MfPatient;

public sealed class PatientBffClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<PatientDto>> ListAsync(CancellationToken cancellationToken)
    {
        var patients = await httpClient.GetFromJsonAsync<IReadOnlyList<PatientDto>>(
            "api/patients?skip=0&take=50",
            cancellationToken);

        return patients ?? [];
    }

    public async Task<ApiMutationResult> CreateAsync(CreatePatientDto request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("api/patients", request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return ApiMutationResult.Ok();
        }

        var error = await ReadErrorAsync(response, cancellationToken);
        return ApiMutationResult.Fail(error.Code, error.Message);
    }

    public async Task<IReadOnlyList<InsurancePlanDto>> ListPlansAsync(CancellationToken cancellationToken)
    {
        var plans = await httpClient.GetFromJsonAsync<IReadOnlyList<InsurancePlanDto>>(
            "api/patients/plans",
            cancellationToken);

        return plans ?? [];
    }

    public async Task<ApiMutationResult> CreatePlanAsync(CreateInsurancePlanDto request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("api/patients/plans", request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return ApiMutationResult.Ok();
        }

        var error = await ReadErrorAsync(response, cancellationToken);
        return ApiMutationResult.Fail(error.Code, error.Message);
    }

    public async Task<ApiMutationResult> AssignPlanAsync(Guid patientId, Guid planId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PutAsync($"api/patients/{patientId}/plans/{planId}", content: null, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return ApiMutationResult.Ok();
        }

        var error = await ReadErrorAsync(response, cancellationToken);
        return ApiMutationResult.Fail(error.Code, error.Message);
    }

    public async Task<IReadOnlyList<PatientPlanDto>> ListPatientPlansAsync(Guid patientId, CancellationToken cancellationToken)
    {
        var plans = await httpClient.GetFromJsonAsync<IReadOnlyList<PatientPlanDto>>(
            $"api/patients/{patientId}/plans",
            cancellationToken);

        return plans ?? [];
    }

    private static async Task<ApiErrorDto> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var validation = await response.Content.ReadFromJsonAsync<ValidationProblemDto>(cancellationToken);
            if (validation?.Errors.Count > 0)
            {
                var message = string.Join(" ", validation.Errors.SelectMany(pair => pair.Value));
                return new ApiErrorDto("validation_error", message);
            }
        }

        return await response.Content.ReadFromJsonAsync<ApiErrorDto>(cancellationToken)
               ?? new ApiErrorDto(((int)response.StatusCode).ToString(), "Nao foi possivel concluir a operacao.");
    }
}

public sealed record PatientDto(Guid Id, Guid TenantId, string FullName, string Document, DateOnly BirthDate);
public sealed record InsurancePlanDto(Guid Id, Guid TenantId, string PayerName, string PlanName, bool Active);
public sealed record PatientPlanDto(Guid PatientId, Guid PlanId, string PayerName, string PlanName, bool Active, DateTimeOffset LinkedAtUtc);

public sealed record CreatePatientDto(string FullName, string Document, DateOnly BirthDate);
public sealed record CreateInsurancePlanDto(string PayerName, string PlanName);

public sealed record ApiMutationResult(bool Success, string? Code, string? Message)
{
    public static ApiMutationResult Ok() => new(true, null, null);
    public static ApiMutationResult Fail(string? code, string? message) => new(false, code, message);
}

public sealed record ApiErrorDto(string Code, string Message);

internal sealed record ValidationProblemDto(Dictionary<string, string[]> Errors);

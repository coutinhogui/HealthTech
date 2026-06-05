using System.Net.Http.Json;

namespace HealthTech.Front.Features.Ehr;

public sealed class EhrBffClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<EhrPatientDto>> ListPatientsAsync(CancellationToken cancellationToken)
    {
        var patients = await httpClient.GetFromJsonAsync<IReadOnlyList<EhrPatientDto>>(
            "api/patients?skip=0&take=200",
            cancellationToken);

        return patients ?? [];
    }

    public async Task<IReadOnlyList<EhrEvolutionDto>> ListEvolutionsAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var evolutions = await httpClient.GetFromJsonAsync<IReadOnlyList<EhrEvolutionDto>>(
            $"api/patients/{patientId:D}/evolutions?skip=0&take=50",
            cancellationToken);

        return evolutions ?? [];
    }

    public async Task<ApiMutationResult> CreateEvolutionAsync(
        Guid patientId,
        CreateEhrEvolutionDto request,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"api/patients/{patientId:D}/evolutions",
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return ApiMutationResult.Ok();
        }

        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(cancellationToken)
                    ?? new ApiErrorDto(((int)response.StatusCode).ToString(), "Nao foi possivel concluir a operacao.");
        return ApiMutationResult.Fail(error.Code, error.Message);
    }
}

public sealed record EhrPatientDto(Guid Id, Guid TenantId, string FullName, string Document, DateOnly BirthDate);

public sealed record EhrEvolutionDto(
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

public sealed record CreateEhrEvolutionDto(
    DateTimeOffset? EncounteredAtUtc,
    string Subjective,
    string Objective,
    string Assessment,
    string Plan);

public sealed record ApiMutationResult(bool Success, string? Code, string? Message)
{
    public static ApiMutationResult Ok() => new(true, null, null);
    public static ApiMutationResult Fail(string? code, string? message) => new(false, code, message);
}

public sealed record ApiErrorDto(string Code, string Message);

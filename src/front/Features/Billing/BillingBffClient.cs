using System.Net.Http.Json;

namespace HealthTech.Front.Features.Billing;

public sealed class BillingBffClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<BillingChargeDto>> ListChargesAsync(string? status, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrWhiteSpace(status)
            ? "api/appointments/charges"
            : $"api/appointments/charges?status={Uri.EscapeDataString(status)}";

        var charges = await httpClient.GetFromJsonAsync<IReadOnlyList<BillingChargeDto>>(path, cancellationToken);
        return charges ?? [];
    }

    public async Task<ApiMutationResult> CreateChargeAsync(CreateBillingChargeDto request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("api/appointments/charges", request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return ApiMutationResult.Ok();
        }

        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(cancellationToken)
                    ?? new ApiErrorDto(((int)response.StatusCode).ToString(), "Nao foi possivel concluir a operacao.");
        return ApiMutationResult.Fail(error.Code, error.Message);
    }

    public async Task<ApiMutationResult> MarkChargeAsPaidAsync(Guid chargeId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsync($"api/appointments/charges/{chargeId:D}/paid", null, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return ApiMutationResult.Ok();
        }

        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(cancellationToken)
                    ?? new ApiErrorDto(((int)response.StatusCode).ToString(), "Nao foi possivel concluir a operacao.");
        return ApiMutationResult.Fail(error.Code, error.Message);
    }

    public async Task<ApiMutationResult> CloseBatchAsync(DateOnly competenceMonth, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/appointments/batches/close",
            new CloseBillingBatchDto(competenceMonth),
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return ApiMutationResult.Ok();
        }

        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(cancellationToken)
                    ?? new ApiErrorDto(((int)response.StatusCode).ToString(), "Nao foi possivel concluir a operacao.");
        return ApiMutationResult.Fail(error.Code, error.Message);
    }

    public async Task<IReadOnlyList<BillingBatchDto>> ListBatchesAsync(CancellationToken cancellationToken)
    {
        var batches = await httpClient.GetFromJsonAsync<IReadOnlyList<BillingBatchDto>>("api/appointments/batches", cancellationToken);
        return batches ?? [];
    }
}

public sealed class BillingPatientOptionsClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<BillingPatientDto>> ListAsync(CancellationToken cancellationToken)
    {
        var patients = await httpClient.GetFromJsonAsync<IReadOnlyList<BillingPatientDto>>("api/patients?skip=0&take=200", cancellationToken);
        return patients ?? [];
    }
}

public sealed class BillingAppointmentOptionsClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<BillingAppointmentDto>> ListAsync(CancellationToken cancellationToken)
    {
        var from = DateTimeOffset.UtcNow.AddDays(-30).ToString("O");
        var to = DateTimeOffset.UtcNow.AddDays(60).ToString("O");
        var appointments = await httpClient.GetFromJsonAsync<IReadOnlyList<BillingAppointmentDto>>(
            $"api/appointments?fromUtc={Uri.EscapeDataString(from)}&toUtc={Uri.EscapeDataString(to)}",
            cancellationToken);
        return appointments ?? [];
    }
}

public sealed record BillingChargeDto(
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

public sealed record BillingBatchDto(
    Guid Id,
    Guid TenantId,
    DateOnly CompetenceMonth,
    decimal TotalAmount,
    string Status,
    DateTimeOffset CreatedAtUtc);

public sealed record CreateBillingChargeDto(
    Guid AppointmentId,
    Guid PatientId,
    string PayerType,
    string Description,
    decimal Amount,
    DateOnly DueDate);

public sealed record CloseBillingBatchDto(DateOnly CompetenceMonth);

public sealed record BillingPatientDto(Guid Id, Guid TenantId, string FullName, string Document, DateOnly BirthDate);

public sealed record BillingAppointmentDto(
    Guid Id,
    Guid TenantId,
    Guid PatientId,
    Guid ProfessionalId,
    Guid? LocationId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string Status,
    string? Notes);

public sealed record ApiMutationResult(bool Success, string? Code, string? Message)
{
    public static ApiMutationResult Ok() => new(true, null, null);
    public static ApiMutationResult Fail(string? code, string? message) => new(false, code, message);
}

public sealed record ApiErrorDto(string Code, string Message);

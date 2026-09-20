using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace HealthTech.Front.Features.Appointment;

public sealed class AppointmentBffClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<AppointmentDto>> ListAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken)
    {
        var query = QueryHelpers.AddQueryString("api/appointments", new Dictionary<string, string?>
        {
            ["fromUtc"] = fromUtc.ToString("O"),
            ["toUtc"] = toUtc.ToString("O")
        });

        var appointments = await httpClient.GetFromJsonAsync<IReadOnlyList<AppointmentDto>>(query, cancellationToken);
        return appointments ?? [];
    }

    public async Task<ApiMutationResult> CreateAsync(CreateAppointmentDto request, CancellationToken cancellationToken)
        => await SendMutationAsync(() => httpClient.PostAsJsonAsync("api/appointments", request, cancellationToken), cancellationToken);

    public async Task<ApiMutationResult> CancelAsync(Guid appointmentId, CancellationToken cancellationToken)
        => await SendMutationAsync(() => httpClient.PostAsync($"api/appointments/{appointmentId:D}/cancel", null, cancellationToken), cancellationToken);

    public async Task<ApiMutationResult> RescheduleAsync(Guid appointmentId, RescheduleAppointmentDto request, CancellationToken cancellationToken)
        => await SendMutationAsync(() => httpClient.PatchAsJsonAsync($"api/appointments/{appointmentId:D}/schedule", request, cancellationToken), cancellationToken);

    public async Task<ApiEntityMutationResult<Guid>> RegisterWhatsappIntentAsync(
        Guid appointmentId,
        RegisterWhatsappIntentDto request,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"api/appointments/{appointmentId:D}/whatsapp/intents",
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<CreatedEntityResponse>(cancellationToken);
            return payload is null
                ? ApiEntityMutationResult<Guid>.Fail("invalid_response", "A API nao retornou o id do registro.")
                : ApiEntityMutationResult<Guid>.Ok(payload.Id);
        }

        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(cancellationToken)
                    ?? new ApiErrorDto(((int)response.StatusCode).ToString(), "Nao foi possivel concluir a operacao.");
        return ApiEntityMutationResult<Guid>.Fail(error.Code, error.Message);
    }

    public async Task<ApiMutationResult> MarkWhatsappSentAsync(
        Guid appointmentId,
        Guid messageId,
        MarkWhatsappSentDto request,
        CancellationToken cancellationToken)
        => await SendMutationAsync(
            () => httpClient.PostAsJsonAsync(
                $"api/appointments/{appointmentId:D}/whatsapp/{messageId:D}/sent",
                request,
                cancellationToken),
            cancellationToken);

    public async Task<IReadOnlyList<ManualWhatsappMessageDto>> ListWhatsappMessagesAsync(
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var messages = await httpClient.GetFromJsonAsync<IReadOnlyList<ManualWhatsappMessageDto>>(
            $"api/appointments/{appointmentId:D}/whatsapp",
            cancellationToken);

        return messages ?? [];
    }

    public async Task<IReadOnlyList<AvailableSlotDto>> ListAvailableSlotsAsync(
        Guid professionalId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int slotMinutes,
        Guid? locationId,
        CancellationToken cancellationToken)
    {
        var queryValues = new Dictionary<string, string?>
        {
            ["professionalId"] = professionalId.ToString("D"),
            ["fromUtc"] = fromUtc.ToString("O"),
            ["toUtc"] = toUtc.ToString("O"),
            ["slotMinutes"] = slotMinutes.ToString()
        };

        if (locationId is not null)
        {
            queryValues["locationId"] = locationId.Value.ToString("D");
        }

        var query = QueryHelpers.AddQueryString("api/appointments/slots", queryValues);
        var slots = await httpClient.GetFromJsonAsync<IReadOnlyList<AvailableSlotDto>>(query, cancellationToken);
        return slots ?? [];
    }

    private static async Task<ApiMutationResult> SendMutationAsync(
        Func<Task<HttpResponseMessage>> sendAsync,
        CancellationToken cancellationToken)
    {
        using var response = await sendAsync();
        if (response.IsSuccessStatusCode)
        {
            return ApiMutationResult.Ok();
        }

        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(cancellationToken)
                    ?? new ApiErrorDto(((int)response.StatusCode).ToString(), "Nao foi possivel concluir a operacao.");
        return ApiMutationResult.Fail(error.Code, error.Message);
    }
}

public sealed class AppointmentPatientOptionsClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<AppointmentPatientOptionDto>> ListAsync(CancellationToken cancellationToken)
    {
        var patients = await httpClient.GetFromJsonAsync<IReadOnlyList<AppointmentPatientOptionDto>>(
            "api/patients?skip=0&take=200",
            cancellationToken);

        return patients ?? [];
    }
}

public sealed class AppointmentProfessionalOptionsClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<AppointmentProfessionalOptionDto>> ListAsync(CancellationToken cancellationToken)
    {
        var professionals = await httpClient.GetFromJsonAsync<IReadOnlyList<AppointmentProfessionalOptionDto>>(
            "api/professionals",
            cancellationToken);

        return professionals ?? [];
    }
}

public sealed class ProfessionalManagementBffClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<AppointmentProfessionalOptionDto>> ListProfessionalsAsync(CancellationToken cancellationToken)
        => await new AppointmentProfessionalOptionsClient(httpClient).ListAsync(cancellationToken);

    public async Task<IReadOnlyList<SpecialtyDto>> ListSpecialtiesAsync(CancellationToken cancellationToken)
    {
        var specialties = await httpClient.GetFromJsonAsync<IReadOnlyList<SpecialtyDto>>("api/specialties", cancellationToken);
        return specialties ?? [];
    }

    public async Task<IReadOnlyList<LocationDto>> ListLocationsAsync(CancellationToken cancellationToken)
    {
        var locations = await httpClient.GetFromJsonAsync<IReadOnlyList<LocationDto>>("api/locations", cancellationToken);
        return locations ?? [];
    }

    public Task<ApiMutationResult> CreateSpecialtyAsync(CreateSpecialtyDto request, CancellationToken cancellationToken)
        => PostAsync("api/specialties", request, cancellationToken);

    public Task<ApiMutationResult> CreateLocationAsync(CreateLocationDto request, CancellationToken cancellationToken)
        => PostAsync("api/locations", request, cancellationToken);

    public Task<ApiMutationResult> CreateProfessionalAsync(CreateProfessionalDto request, CancellationToken cancellationToken)
        => PostAsync("api/professionals", request, cancellationToken);

    private async Task<ApiMutationResult> PostAsync<TRequest>(string path, TRequest request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(path, request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return ApiMutationResult.Ok();
        }

        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(cancellationToken)
                    ?? new ApiErrorDto(((int)response.StatusCode).ToString(), "Nao foi possivel concluir a operacao.");
        return ApiMutationResult.Fail(error.Code, error.Message);
    }
}

public sealed record AppointmentPatientOptionDto(Guid Id, Guid TenantId, string FullName, string Document, DateOnly BirthDate);
public sealed record AppointmentProfessionalOptionDto(Guid Id, string FullName, string SpecialtyName, bool Active);
public sealed record SpecialtyDto(Guid Id, string Name, bool Active);
public sealed record LocationDto(Guid Id, string Name, string Timezone, bool Active);
public sealed record CreateSpecialtyDto(string Name);
public sealed record CreateLocationDto(string Name, string Timezone);
public sealed record CreateProfessionalDto(string FullName, Guid SpecialtyId);

public sealed record AppointmentDto(
    Guid Id,
    Guid TenantId,
    Guid PatientId,
    Guid ProfessionalId,
    Guid? LocationId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string Status,
    string? Notes);

public sealed record AvailableSlotDto(
    Guid ProfessionalId,
    Guid? LocationId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);

public sealed record CreateAppointmentDto(
    Guid PatientId,
    Guid ProfessionalId,
    Guid? LocationId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Notes);

public sealed record RescheduleAppointmentDto(
    Guid ProfessionalId,
    Guid? LocationId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Notes);

public sealed record RegisterWhatsappIntentDto(string RecipientPhone, string MessageText);
public sealed record MarkWhatsappSentDto(string? Provider, string? ProviderMessageId);
public sealed record ManualWhatsappMessageDto(
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

public sealed record ApiMutationResult(bool Success, string? Code, string? Message)
{
    public static ApiMutationResult Ok() => new(true, null, null);
    public static ApiMutationResult Fail(string? code, string? message) => new(false, code, message);
}

public sealed record ApiEntityMutationResult<T>(bool Success, T? Value, string? Code, string? Message)
{
    public static ApiEntityMutationResult<T> Ok(T value) => new(true, value, null, null);
    public static ApiEntityMutationResult<T> Fail(string? code, string? message) => new(false, default, code, message);
}

public sealed record ApiErrorDto(string Code, string Message);
internal sealed record CreatedEntityResponse(Guid Id);

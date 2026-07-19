using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace HealthTech.Front.Features.Discovery;

public sealed class DiscoveryBffClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<DiscoverySearchResultDto>> SearchAsync(
        string mode,
        string? query,
        string? clinic,
        string? specialty,
        string? region,
        double? latitude,
        double? longitude,
        int take,
        CancellationToken cancellationToken)
    {
        var queryValues = new Dictionary<string, string?>
        {
            ["mode"] = mode,
            ["query"] = string.IsNullOrWhiteSpace(query) ? null : query.Trim(),
            ["clinic"] = string.IsNullOrWhiteSpace(clinic) ? null : clinic.Trim(),
            ["specialty"] = string.IsNullOrWhiteSpace(specialty) ? null : specialty.Trim(),
            ["region"] = string.IsNullOrWhiteSpace(region) ? null : region.Trim(),
            ["latitude"] = latitude?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["longitude"] = longitude?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["take"] = take.ToString()
        };

        var requestUri = QueryHelpers.AddQueryString("api/discovery/search", queryValues);
        return await httpClient.GetFromJsonAsync<IReadOnlyList<DiscoverySearchResultDto>>(requestUri, cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<DiscoveryClinicDto>> ListClinicsAsync(CancellationToken cancellationToken)
        => await httpClient.GetFromJsonAsync<IReadOnlyList<DiscoveryClinicDto>>("api/discovery/clinics", cancellationToken) ?? [];

    public async Task<IReadOnlyList<DiscoverySpecialtyDto>> ListSpecialtiesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var query = QueryHelpers.AddQueryString("api/discovery/specialties", new Dictionary<string, string?>
        {
            ["tenantId"] = tenantId.ToString("D")
        });
        return await httpClient.GetFromJsonAsync<IReadOnlyList<DiscoverySpecialtyDto>>(query, cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<DiscoveryProfessionalDto>> ListProfessionalsAsync(
        Guid tenantId,
        Guid? specialtyId,
        CancellationToken cancellationToken)
    {
        var queryValues = new Dictionary<string, string?>
        {
            ["tenantId"] = tenantId.ToString("D")
        };
        if (specialtyId is not null)
        {
            queryValues["specialtyId"] = specialtyId.Value.ToString("D");
        }

        var query = QueryHelpers.AddQueryString("api/discovery/professionals", queryValues);
        return await httpClient.GetFromJsonAsync<IReadOnlyList<DiscoveryProfessionalDto>>(query, cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<DiscoverySlotDto>> ListSlotsAsync(
        Guid tenantId,
        Guid professionalId,
        Guid? locationId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int slotMinutes,
        CancellationToken cancellationToken)
    {
        var query = QueryHelpers.AddQueryString("api/discovery/slots", new Dictionary<string, string?>
        {
            ["tenantId"] = tenantId.ToString("D"),
            ["professionalId"] = professionalId.ToString("D"),
            ["locationId"] = locationId?.ToString("D"),
            ["fromUtc"] = fromUtc.ToString("O"),
            ["toUtc"] = toUtc.ToString("O"),
            ["slotMinutes"] = slotMinutes.ToString()
        });
        return await httpClient.GetFromJsonAsync<IReadOnlyList<DiscoverySlotDto>>(query, cancellationToken) ?? [];
    }

    public async Task<DiscoveryBookingMutationResult> BookAsync(DiscoveryBookingRequest request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("api/discovery/appointments", request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<DiscoveryBookingCreatedResponse>(cancellationToken);
            return payload is null
                ? DiscoveryBookingMutationResult.Fail("invalid_response", "Nao foi possivel confirmar o agendamento.")
                : DiscoveryBookingMutationResult.Ok(payload.Id);
        }

        var error = await response.Content.ReadFromJsonAsync<DiscoveryErrorResponse>(cancellationToken);
        return DiscoveryBookingMutationResult.Fail(
            error?.Error ?? ((int)response.StatusCode).ToString(),
            error?.Message ?? "Nao foi possivel concluir o agendamento.");
    }
}

public sealed record DiscoveryClinicDto(Guid Id, string Name, bool Active);
public sealed record DiscoverySpecialtyDto(Guid Id, string Name, bool Active);
public sealed record DiscoveryProfessionalDto(Guid Id, string FullName, Guid SpecialtyId, string SpecialtyName, bool Active);
public sealed record DiscoverySlotDto(Guid ProfessionalId, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc);
public sealed record DiscoverySearchResultDto(
    Guid TenantId,
    Guid ProfessionalId,
    Guid? LocationId,
    string ProfessionalName,
    string SpecialtyName,
    string ClinicName,
    string? LocationName,
    string? City,
    string? State,
    string RegionLabel,
    IReadOnlyList<DiscoverySlotDto> NextSlots);

public sealed record DiscoveryBookingRequest(
    Guid TenantId,
    Guid ProfessionalId,
    Guid? LocationId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string PatientName,
    string PatientDocument,
    DateOnly PatientBirthDate,
    string? PatientEmail,
    string? PatientPhone,
    string? Notes);

public sealed record DiscoveryBookingMutationResult(bool Success, Guid? AppointmentId, string? Code, string? Message)
{
    public static DiscoveryBookingMutationResult Ok(Guid appointmentId) => new(true, appointmentId, null, null);
    public static DiscoveryBookingMutationResult Fail(string code, string message) => new(false, null, code, message);
}

internal sealed record DiscoveryBookingCreatedResponse(Guid Id);
internal sealed record DiscoveryErrorResponse(string Error, string? Message);

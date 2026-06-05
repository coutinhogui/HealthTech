using System.Net;
using System.Net.Http.Json;
using Xunit;
using AppointmentBffClient = HealthTech.Front.Features.Appointment.AppointmentBffClient;
using AvailableSlotDto = HealthTech.Front.Features.Appointment.AvailableSlotDto;
using AppointmentDto = HealthTech.Front.Features.Appointment.AppointmentDto;
using AppointmentPatientOptionDto = HealthTech.Front.Features.Appointment.AppointmentPatientOptionDto;
using AppointmentPatientOptionsClient = HealthTech.Front.Features.Appointment.AppointmentPatientOptionsClient;
using AppointmentProfessionalOptionDto = HealthTech.Front.Features.Appointment.AppointmentProfessionalOptionDto;
using AppointmentProfessionalOptionsClient = HealthTech.Front.Features.Appointment.AppointmentProfessionalOptionsClient;
using CreateProfessionalDto = HealthTech.Front.Features.Appointment.CreateProfessionalDto;
using RegisterWhatsappIntentDto = HealthTech.Front.Features.Appointment.RegisterWhatsappIntentDto;
using MarkWhatsappSentDto = HealthTech.Front.Features.Appointment.MarkWhatsappSentDto;
using ManualWhatsappMessageDto = HealthTech.Front.Features.Appointment.ManualWhatsappMessageDto;
using ProfessionalManagementBffClient = HealthTech.Front.Features.Appointment.ProfessionalManagementBffClient;
using RescheduleAppointmentDto = HealthTech.Front.Features.Appointment.RescheduleAppointmentDto;
using CreatePatientDto = HealthTech.Front.Features.Patient.CreatePatientDto;
using PatientApiErrorDto = HealthTech.Front.Features.Patient.ApiErrorDto;
using PatientBffClient = HealthTech.Front.Features.Patient.PatientBffClient;
using PatientDto = HealthTech.Front.Features.Patient.PatientDto;
using CreateEhrEvolutionDto = HealthTech.Front.Features.Ehr.CreateEhrEvolutionDto;
using EhrBffClient = HealthTech.Front.Features.Ehr.EhrBffClient;
using EhrPatientDto = HealthTech.Front.Features.Ehr.EhrPatientDto;
using EhrEvolutionDto = HealthTech.Front.Features.Ehr.EhrEvolutionDto;
using BillingBffClient = HealthTech.Front.Features.Billing.BillingBffClient;
using BillingChargeDto = HealthTech.Front.Features.Billing.BillingChargeDto;
using CreateBillingChargeDto = HealthTech.Front.Features.Billing.CreateBillingChargeDto;
using DiscoveryBffClient = HealthTech.Front.Features.Discovery.DiscoveryBffClient;
using DiscoverySearchResultDto = HealthTech.Front.Features.Discovery.DiscoverySearchResultDto;
using DiscoverySlotDto = HealthTech.Front.Features.Discovery.DiscoverySlotDto;

namespace HealthTech.FrontendClients.Tests;

public sealed class FrontendClientTests
{
    [Fact]
    public async Task Patient_client_lists_patients_from_bff_contract()
    {
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[]
            {
                new PatientDto(Guid.Parse("11111111-1111-1111-1111-111111111111"), Guid.NewGuid(), "Maria Araujo", "123", new DateOnly(1990, 1, 1))
            })
        });
        var client = new PatientBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var patients = await client.ListAsync(CancellationToken.None);

        Assert.Equal("https://bff.local/api/patients?skip=0&take=50", handler.LastRequestUri?.ToString());
        Assert.Single(patients);
        Assert.Equal("Maria Araujo", patients[0].FullName);
    }

    [Fact]
    public async Task Patient_client_maps_conflict_response()
    {
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = JsonContent.Create(new PatientApiErrorDto("patient_document_conflict", "A patient with this document already exists."))
        });
        var client = new PatientBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var result = await client.CreateAsync(new CreatePatientDto("Maria", "123", new DateOnly(1990, 1, 1)), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("patient_document_conflict", result.Code);
    }

    [Fact]
    public async Task Appointment_client_lists_appointments_with_window_query()
    {
        var from = new DateTimeOffset(2026, 5, 26, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddDays(1);
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(Array.Empty<AppointmentDto>())
        });
        var client = new AppointmentBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var appointments = await client.ListAsync(from, to, CancellationToken.None);

        Assert.Empty(appointments);
        Assert.Equal(
            "https://bff.local/api/appointments?fromUtc=2026-05-26T00%3A00%3A00.0000000%2B00%3A00&toUtc=2026-05-27T00%3A00%3A00.0000000%2B00%3A00",
            handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task Appointment_client_cancels_appointment()
    {
        var appointmentId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = new AppointmentBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var result = await client.CancelAsync(appointmentId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(HttpMethod.Post, handler.LastMethod);
        Assert.Equal("https://bff.local/api/appointments/eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee/cancel", handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task Appointment_client_reschedules_appointment()
    {
        var appointmentId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        var professionalId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var locationId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var startsAt = new DateTimeOffset(2026, 5, 30, 13, 0, 0, TimeSpan.Zero);
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = new AppointmentBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var result = await client.RescheduleAsync(
            appointmentId,
            new RescheduleAppointmentDto(professionalId, locationId, startsAt, startsAt.AddMinutes(30), "Retorno"),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(new HttpMethod("PATCH"), handler.LastMethod);
        Assert.Equal("https://bff.local/api/appointments/eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee/schedule", handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task Appointment_client_requests_available_slots_for_professional_and_location()
    {
        var professionalId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var locationId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var from = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero);
        var to = from.AddHours(4);
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[]
            {
                new AvailableSlotDto(professionalId, locationId, from, from.AddMinutes(30))
            })
        });
        var client = new AppointmentBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var slots = await client.ListAvailableSlotsAsync(professionalId, from, to, 30, locationId, CancellationToken.None);

        Assert.Single(slots);
        Assert.Equal(
            "https://bff.local/api/appointments/slots?professionalId=cccccccc-cccc-cccc-cccc-cccccccccccc&fromUtc=2026-06-01T08%3A00%3A00.0000000%2B00%3A00&toUtc=2026-06-01T12%3A00%3A00.0000000%2B00%3A00&slotMinutes=30&locationId=33333333-3333-3333-3333-333333333333",
            handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task Appointment_client_registers_whatsapp_intent_and_reads_created_id()
    {
        var appointmentId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        var createdId = Guid.Parse("abababab-abab-abab-abab-abababababab");
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new { id = createdId })
        });
        var client = new AppointmentBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var result = await client.RegisterWhatsappIntentAsync(
            appointmentId,
            new RegisterWhatsappIntentDto("+5511999999999", "Confirmacao manual"),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(createdId, result.Value);
        Assert.Equal("https://bff.local/api/appointments/eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee/whatsapp/intents", handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task Appointment_client_lists_whatsapp_messages_for_appointment()
    {
        var appointmentId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[]
            {
                new ManualWhatsappMessageDto(
                    Guid.Parse("abababab-abab-abab-abab-abababababab"),
                    appointmentId,
                    "+5511999999999",
                    "Confirmacao manual",
                    "intent",
                    null,
                    null,
                    "user-1",
                    new DateTimeOffset(2026, 6, 1, 13, 0, 0, TimeSpan.Zero),
                    null)
            })
        });
        var client = new AppointmentBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var messages = await client.ListWhatsappMessagesAsync(appointmentId, CancellationToken.None);

        Assert.Single(messages);
        Assert.Equal("https://bff.local/api/appointments/eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee/whatsapp", handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task Appointment_client_marks_whatsapp_message_as_sent()
    {
        var appointmentId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        var messageId = Guid.Parse("abababab-abab-abab-abab-abababababab");
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = new AppointmentBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var result = await client.MarkWhatsappSentAsync(
            appointmentId,
            messageId,
            new MarkWhatsappSentDto("whatsapp-cloud-api", "provider-1"),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("https://bff.local/api/appointments/eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee/whatsapp/abababab-abab-abab-abab-abababababab/sent", handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task Appointment_patient_options_client_reads_patients_for_select()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[]
            {
                new AppointmentPatientOptionDto(patientId, Guid.NewGuid(), "Helena Costa", "321", new DateOnly(1985, 5, 1))
            })
        });
        var client = new AppointmentPatientOptionsClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var options = await client.ListAsync(CancellationToken.None);

        Assert.Equal("https://bff.local/api/patients?skip=0&take=200", handler.LastRequestUri?.ToString());
        Assert.Equal(patientId, options[0].Id);
        Assert.Equal("Helena Costa", options[0].FullName);
    }

    [Fact]
    public async Task Appointment_professional_options_client_reads_professionals_for_select()
    {
        var professionalId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[]
            {
                new AppointmentProfessionalOptionDto(professionalId, "Dra. Helena Costa", "Clinica Geral", true)
            })
        });
        var client = new AppointmentProfessionalOptionsClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var options = await client.ListAsync(CancellationToken.None);

        Assert.Equal("https://bff.local/api/professionals", handler.LastRequestUri?.ToString());
        Assert.Equal(professionalId, options[0].Id);
        Assert.Equal("Dra. Helena Costa", options[0].FullName);
    }

    [Fact]
    public async Task Professional_management_client_creates_professional()
    {
        var specialtyId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var handler = new CapturingHandler(request => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new { id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc") })
        });
        var client = new ProfessionalManagementBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var result = await client.CreateProfessionalAsync(new CreateProfessionalDto("Dra. Helena Costa", specialtyId), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("https://bff.local/api/professionals", handler.LastRequestUri?.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastMethod);
    }

    [Fact]
    public async Task Ehr_client_lists_patients_and_evolutions()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var call = 0;
        var handler = new CapturingHandler(request =>
        {
            call++;
            return call switch
            {
                1 => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new[]
                    {
                        new EhrPatientDto(patientId, tenantId, "Maria Araujo", "123", new DateOnly(1990, 1, 1))
                    })
                },
                _ => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new[]
                    {
                        new EhrEvolutionDto(
                            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                            tenantId,
                            patientId,
                            new DateTimeOffset(2026, 6, 1, 13, 0, 0, TimeSpan.Zero),
                            "subj",
                            "obj",
                            "assess",
                            "plan",
                            "user-1",
                            new DateTimeOffset(2026, 6, 1, 13, 5, 0, TimeSpan.Zero))
                    })
                }
            };
        });

        var client = new EhrBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var patients = await client.ListPatientsAsync(CancellationToken.None);
        Assert.Single(patients);
        Assert.Equal("https://bff.local/api/patients?skip=0&take=200", handler.LastRequestUri?.ToString());

        var evolutions = await client.ListEvolutionsAsync(patientId, CancellationToken.None);
        Assert.Single(evolutions);
        Assert.Equal("https://bff.local/api/patients/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/evolutions?skip=0&take=50", handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task Ehr_client_creates_evolution()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new { id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") })
        });
        var client = new EhrBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var result = await client.CreateEvolutionAsync(
            patientId,
            new CreateEhrEvolutionDto(
                new DateTimeOffset(2026, 6, 1, 13, 0, 0, TimeSpan.Zero),
                "subj",
                "obj",
                "assess",
                "plan"),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("https://bff.local/api/patients/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/evolutions", handler.LastRequestUri?.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastMethod);
    }

    [Fact]
    public async Task Billing_client_lists_charges_with_status_filter()
    {
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[]
            {
                new BillingChargeDto(
                    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    "insurance",
                    "Consulta",
                    180m,
                    new DateOnly(2026, 6, 30),
                    "open",
                    null,
                    null,
                    new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero))
            })
        });
        var client = new BillingBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var charges = await client.ListChargesAsync("open", CancellationToken.None);

        Assert.Single(charges);
        Assert.Equal("https://bff.local/api/appointments/charges?status=open", handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task Billing_client_creates_charge()
    {
        var appointmentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var patientId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new { id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd") })
        });
        var client = new BillingBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var result = await client.CreateChargeAsync(
            new CreateBillingChargeDto(appointmentId, patientId, "insurance", "Consulta", 200m, new DateOnly(2026, 6, 30)),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(HttpMethod.Post, handler.LastMethod);
        Assert.Equal("https://bff.local/api/appointments/charges", handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task Discovery_client_searches_with_mode_query_region_and_take()
    {
        var professionalId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[]
            {
                new DiscoverySearchResultDto(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    professionalId,
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    "Dra. Ana Cardoso",
                    "Cardiologia",
                    "Demo Clinic A",
                    "Unidade Centro",
                    "Sao Paulo",
                    "SP",
                    "Centro, Sao Paulo - SP",
                    [new DiscoverySlotDto(professionalId, DateTimeOffset.Parse("2026-06-05T12:00:00Z"), DateTimeOffset.Parse("2026-06-05T12:30:00Z"))])
            })
        });
        var client = new DiscoveryBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        var results = await client.SearchAsync(
            "professional",
            "cardio",
            clinic: null,
            specialty: "Cardiologia",
            region: "Centro",
            latitude: null,
            longitude: null,
            take: 8,
            cancellationToken: CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Dra. Ana Cardoso", results[0].ProfessionalName);
        Assert.Equal(
            "https://bff.local/api/discovery/search?mode=professional&query=cardio&specialty=Cardiologia&region=Centro&take=8",
            handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task Discovery_client_sends_coordinates_when_patient_uses_current_location()
    {
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(Array.Empty<DiscoverySearchResultDto>())
        });
        var client = new DiscoveryBffClient(new HttpClient(handler) { BaseAddress = new Uri("https://bff.local/") });

        await client.SearchAsync(
            "professional",
            "cardio",
            clinic: null,
            specialty: "Cardiologia",
            region: null,
            latitude: -23.561414,
            longitude: -46.655881,
            take: 8,
            cancellationToken: CancellationToken.None);

        Assert.Equal(
            "https://bff.local/api/discovery/search?mode=professional&query=cardio&specialty=Cardiologia&latitude=-23.561414&longitude=-46.655881&take=8",
            handler.LastRequestUri?.ToString());
    }

    private sealed class CapturingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }
        public HttpMethod? LastMethod { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            LastMethod = request.Method;
            return Task.FromResult(responder(request));
        }
    }
}

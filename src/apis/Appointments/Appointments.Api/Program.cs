using HealthTech.Appointments.Application;
using HealthTech.Appointments.Infrastructure;
using HealthTech.BuildingBlocks.Abstractions;
using MediatR;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateAppointmentHandler>());
builder.Services.AddAppointmentsInfra();
builder.Services.AddHealthTechJwtAuthentication(builder.Configuration);
builder.Services.AddHealthTechRequestContext(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Appointments API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseHealthTechRequestContext();

app.MapHealthChecks("/health").AllowAnonymous();

var appointments = app.MapGroup("/api/appointments").RequireAuthorization();
var professionals = app.MapGroup("/api/professionals").RequireAuthorization();
var specialties = app.MapGroup("/api/specialties").RequireAuthorization();
var locations = app.MapGroup("/api/locations").RequireAuthorization();
var billing = app.MapGroup("/api/appointments").RequireAuthorization();

appointments.MapPost("/", async (CreateAppointmentCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.Success
        ? Results.Created($"/api/appointments/{result.Value}", new { id = result.Value })
        : ToHttpResult(result.Error);
});

appointments.MapGet("/", async (DateTimeOffset? fromUtc, DateTimeOffset? toUtc, ISender sender) =>
{
    var result = await sender.Send(new ListAppointmentsQuery(fromUtc, toUtc));
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
});

appointments.MapGet("/slots", async (
    Guid professionalId,
    DateTimeOffset? fromUtc,
    DateTimeOffset? toUtc,
    int? slotMinutes,
    Guid? locationId,
    ISender sender) =>
{
    var result = await sender.Send(new ListAvailableSlotsQuery(
        professionalId,
        fromUtc,
        toUtc,
        slotMinutes ?? 30,
        locationId));
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
});

appointments.MapPost("/{appointmentId:guid}/cancel", async (Guid appointmentId, ISender sender) =>
{
    var result = await sender.Send(new CancelAppointmentCommand(appointmentId));
    return result.Success ? Results.NoContent() : ToHttpResult(result.Error);
});

appointments.MapPatch("/{appointmentId:guid}/schedule", async (Guid appointmentId, RescheduleAppointmentRequest request, ISender sender) =>
{
    var result = await sender.Send(new RescheduleAppointmentCommand(
        appointmentId,
        request.ProfessionalId,
        request.LocationId,
        request.StartsAtUtc,
        request.EndsAtUtc,
        request.Notes));
    return result.Success ? Results.NoContent() : ToHttpResult(result.Error);
});

appointments.MapPost("/{appointmentId:guid}/whatsapp/intents", async (Guid appointmentId, RegisterWhatsappIntentRequest request, ISender sender) =>
{
    var result = await sender.Send(new RegisterManualWhatsappIntentCommand(
        appointmentId,
        request.RecipientPhone,
        request.MessageText));

    return result.Success
        ? Results.Created($"/api/appointments/{appointmentId:D}/whatsapp/{result.Value:D}", new { id = result.Value })
        : ToHttpResult(result.Error);
});

appointments.MapPost("/{appointmentId:guid}/whatsapp/{messageId:guid}/sent", async (
    Guid appointmentId,
    Guid messageId,
    MarkWhatsappSentRequest request,
    ISender sender) =>
{
    var result = await sender.Send(new MarkManualWhatsappSentCommand(
        appointmentId,
        messageId,
        request.Provider,
        request.ProviderMessageId));

    return result.Success ? Results.NoContent() : ToHttpResult(result.Error);
});

appointments.MapGet("/{appointmentId:guid}/whatsapp", async (Guid appointmentId, ISender sender) =>
{
    var result = await sender.Send(new ListManualWhatsappMessagesQuery(appointmentId));
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
});

billing.MapPost("/charges", async (CreateChargeRequest request, ISender sender) =>
{
    var result = await sender.Send(new CreateChargeCommand(
        request.AppointmentId,
        request.PatientId,
        request.PayerType,
        request.Description,
        request.Amount,
        request.DueDate));

    return result.Success
        ? Results.Created($"/api/appointments/charges/{result.Value:D}", new { id = result.Value })
        : ToHttpResult(result.Error);
});

billing.MapGet("/charges", async (string? status, ISender sender) =>
{
    var result = await sender.Send(new ListChargesQuery(status));
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
});

billing.MapPost("/charges/{chargeId:guid}/paid", async (Guid chargeId, ISender sender) =>
{
    var result = await sender.Send(new MarkChargeAsPaidCommand(chargeId));
    return result.Success ? Results.NoContent() : ToHttpResult(result.Error);
});

billing.MapPost("/batches/close", async (CloseBillingBatchRequest request, ISender sender) =>
{
    var result = await sender.Send(new CloseBillingBatchCommand(request.CompetenceMonth));
    return result.Success
        ? Results.Created($"/api/appointments/batches/{result.Value:D}", new { id = result.Value })
        : ToHttpResult(result.Error);
});

billing.MapGet("/batches", async (ISender sender) =>
{
    var result = await sender.Send(new ListBillingBatchesQuery());
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
});

professionals.MapGet("/", async (ISender sender) =>
{
    var result = await sender.Send(new ListProfessionalsQuery());
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
});

professionals.MapPost("/", async (CreateProfessionalCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.Success
        ? Results.Created($"/api/professionals/{result.Value}", new { id = result.Value })
        : ToHttpResult(result.Error);
});

specialties.MapGet("/", async (ISender sender) =>
{
    var result = await sender.Send(new ListSpecialtiesQuery());
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
});

specialties.MapPost("/", async (CreateSpecialtyCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.Success
        ? Results.Created($"/api/specialties/{result.Value}", new { id = result.Value })
        : ToHttpResult(result.Error);
});

locations.MapGet("/", async (ISender sender) =>
{
    var result = await sender.Send(new ListLocationsQuery());
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
});

locations.MapPost("/", async (CreateLocationCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.Success
        ? Results.Created($"/api/locations/{result.Value}", new { id = result.Value })
        : ToHttpResult(result.Error);
});

app.Run();

static IResult ToHttpResult(HealthTech.BuildingBlocks.SharedKernel.ResultError? error)
{
    if (error is null)
    {
        return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
    }

    return error.Code switch
    {
        "validation_error" => Results.ValidationProblem(error.Details),
        "appointment_conflict" => Results.Conflict(error),
        "patient_not_found" or "professional_not_found" or "specialty_not_found" or "location_not_found" or "appointment_not_found" or "whatsapp_message_not_found" or "charge_not_found" => Results.NotFound(error),
        "tenant_required" => Results.BadRequest(error),
        "role_forbidden" => Results.Forbid(),
        _ => Results.BadRequest(error)
    };
}

public sealed record RescheduleAppointmentRequest(
    Guid ProfessionalId,
    Guid? LocationId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Notes);

public sealed record RegisterWhatsappIntentRequest(string RecipientPhone, string MessageText);

public sealed record MarkWhatsappSentRequest(string? Provider, string? ProviderMessageId);

public sealed record CreateChargeRequest(
    Guid AppointmentId,
    Guid PatientId,
    string PayerType,
    string Description,
    decimal Amount,
    DateOnly DueDate);

public sealed record CloseBillingBatchRequest(DateOnly CompetenceMonth);

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
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseHealthTechRequestContext();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();

var appointments = app.MapGroup("/api/appointments").RequireAuthorization();

appointments.MapPost("/", async (CreateAppointmentCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.Success
        ? Results.Created($"/api/appointments/{result.Value}", new { id = result.Value })
        : Results.BadRequest(new { error = result.Error });
});

appointments.MapGet("/", async (DateTimeOffset? fromUtc, DateTimeOffset? toUtc, ISender sender) =>
{
    var result = await sender.Send(new ListAppointmentsQuery(fromUtc, toUtc));
    return result.Success ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
});

app.Run();

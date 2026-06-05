using HealthTech.BuildingBlocks.Abstractions;
using HealthTech.Patients.Application;
using HealthTech.Patients.Infrastructure;
using MediatR;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterPatientHandler>());
builder.Services.AddPatientsInfra();
builder.Services.AddHealthTechJwtAuthentication(builder.Configuration);
builder.Services.AddHealthTechRequestContext(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Patients API", Version = "v1" });

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

var patients = app.MapGroup("/api/patients").RequireAuthorization();

patients.MapPost("/", async (RegisterPatientCommand cmd, ISender sender) =>
{
    var result = await sender.Send(cmd);
    return result.Success
        ? Results.Created($"/api/patients/{result.Value}", new { id = result.Value })
        : ToHttpResult(result.Error);
});

patients.MapGet("/{patientId:guid}", async (Guid patientId, ISender sender) =>
{
    var result = await sender.Send(new GetPatientByIdQuery(patientId));
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
});

patients.MapGet("/", async (int? skip, int? take, ISender sender) =>
{
    var result = await sender.Send(new ListPatientsQuery(skip ?? 0, take ?? 50));
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
});

patients.MapPost("/plans", async (CreateInsurancePlanCommand cmd, ISender sender) =>
{
    var result = await sender.Send(cmd);
    return result.Success
        ? Results.Created($"/api/patients/plans/{result.Value}", new { id = result.Value })
        : ToHttpResult(result.Error);
});

patients.MapGet("/plans", async (ISender sender) =>
{
    var result = await sender.Send(new ListInsurancePlansQuery());
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
});

patients.MapPut("/{patientId:guid}/plans/{planId:guid}", async (Guid patientId, Guid planId, ISender sender) =>
{
    var result = await sender.Send(new AssignPatientPlanCommand(patientId, planId));
    return result.Success ? Results.NoContent() : ToHttpResult(result.Error);
});

patients.MapGet("/{patientId:guid}/plans", async (Guid patientId, ISender sender) =>
{
    var result = await sender.Send(new ListPatientPlansQuery(patientId));
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
});

patients.MapPost("/{patientId:guid}/evolutions", async (Guid patientId, CreatePatientEvolutionRequest request, ISender sender) =>
{
    var result = await sender.Send(new CreatePatientEvolutionCommand(
        patientId,
        request.EncounteredAtUtc,
        request.Subjective,
        request.Objective,
        request.Assessment,
        request.Plan));

    return result.Success
        ? Results.Created($"/api/patients/{patientId:D}/evolutions/{result.Value:D}", new { id = result.Value })
        : ToHttpResult(result.Error);
});

patients.MapGet("/{patientId:guid}/evolutions", async (Guid patientId, int? skip, int? take, ISender sender) =>
{
    var result = await sender.Send(new ListPatientEvolutionsQuery(patientId, skip ?? 0, take ?? 50));
    return result.Success ? Results.Ok(result.Value) : ToHttpResult(result.Error);
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
        "patient_document_conflict" => Results.Conflict(error),
        "plan_conflict" => Results.Conflict(error),
        "patient_not_found" => Results.NotFound(error),
        "plan_not_found" => Results.NotFound(error),
        "tenant_required" => Results.BadRequest(error),
        "role_forbidden" => Results.Forbid(),
        _ => Results.BadRequest(error)
    };
}

public sealed record CreatePatientEvolutionRequest(
    DateTimeOffset? EncounteredAtUtc,
    string Subjective,
    string Objective,
    string Assessment,
    string Plan);

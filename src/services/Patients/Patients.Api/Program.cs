using MediatR;
using HealthTech.Patients.Application;
using HealthTech.Patients.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models; // ✅ correct namespace


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterPatientHandler>());

builder.Services.AddPatientsInfra();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.Authority = builder.Configuration["Auth:Authority"]; // Supabase JWKS/Issuer
        opt.TokenValidationParameters.ValidateAudience = false;
        opt.TokenValidationParameters.ValidIssuer = builder.Configuration["Auth:Issuer"]; 
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Patients API", Version = "v1" }));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/patients", async (RegisterPatientCommand cmd, ISender sender) =>
{
    var result = await sender.Send(cmd);
    return result.Success ? Results.Created($"/patients/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
}).RequireAuthorization();

app.Run();

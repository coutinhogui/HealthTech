param(
  [string]$SolutionName = "HealthTech",
  [string]$Company = "Acme",
  [string]$Domain = "healthtech.local",
  [string]$DotnetVersion = "9.0"
)

$ErrorActionPreference = "Stop"

function Ensure-Dir($path) {
  if (!(Test-Path $path)) { New-Item -ItemType Directory -Path $path | Out-Null }
}

function Write-File($path, [string]$content) {
  $dir = Split-Path $path -Parent
  Ensure-Dir $dir
  Set-Content -Path $path -Value $content -Encoding UTF8
}

function Append-File($path, [string]$content) {
  Add-Content -Path $path -Value $content -Encoding UTF8
}

function Dotnet-New($template, $output, $name) {
  Ensure-Dir $output
  dotnet new $template -n $name -o $output | Out-Null
}

# 1) Raiz e solução
$root = (Resolve-Path ".").Path
$src = Join-Path $root "src"
$services = Join-Path $src "services"
$building = Join-Path $src "building-blocks"
$fronts = Join-Path $src "frontends"
$apphost = Join-Path $src "apphost"
$gateway = Join-Path $src "gateway"

Ensure-Dir $src
Ensure-Dir $services
Ensure-Dir $building
Ensure-Dir $fronts
Ensure-Dir $apphost
Ensure-Dir $gateway

if (!(Test-Path (Join-Path $root "$SolutionName.sln"))) {
  dotnet new sln -n $SolutionName | Out-Null
}

# 2) Directory.Build.props
$props = @"
<Project>
  <PropertyGroup>
    <Company>$Company</Company>
    <Authors>$Company</Authors>
    <TargetFramework>net$DotnetVersion</TargetFramework>
    <LangVersion>preview</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <Deterministic>true</Deterministic>
  </PropertyGroup>
</Project>
"@
Write-File (Join-Path $root "Directory.Build.props") $props

# 3) Building Blocks
$sharedKernel = Join-Path $building "SharedKernel"
$abstractions = Join-Path $building "Abstractions"
Dotnet-New classlib $sharedKernel "$SolutionName.BuildingBlocks.SharedKernel"
Dotnet-New classlib $abstractions "$SolutionName.BuildingBlocks.Abstractions"

dotnet sln add "$sharedKernel/$SolutionName.BuildingBlocks.SharedKernel.csproj" | Out-Null

dotnet sln add "$abstractions/$SolutionName.BuildingBlocks.Abstractions.csproj" | Out-Null

# 4) Pacotes comuns
function Add-Common-Packages($proj) {
  dotnet add $proj package MediatR | Out-Null
  dotnet add $proj package FluentValidation | Out-Null
}

# 5) Serviços (exemplo: Patients, Appointments, Identity)
$svcNames = @("Patients","Appointments","Identity")
foreach ($svc in $svcNames) {
  $svcRoot = Join-Path $services $svc
  $projApi = Join-Path $svcRoot "$svc.Api"
  $projApp = Join-Path $svcRoot "$svc.Application"
  $projDom = Join-Path $svcRoot "$svc.Domain"
  $projInfra = Join-Path $svcRoot "$svc.Infrastructure"

  Dotnet-New webapi $projApi "$SolutionName.$svc.Api"
  Dotnet-New classlib $projApp "$SolutionName.$svc.Application"
  Dotnet-New classlib $projDom "$SolutionName.$svc.Domain"
  Dotnet-New classlib $projInfra "$SolutionName.$svc.Infrastructure"

  # Limpar WeatherForecast padrão, se existir
  Remove-Item -Force -Recurse "$projApi/WeatherForecast.cs" -ErrorAction SilentlyContinue
  Remove-Item -Force -Recurse "$projApi/Controllers" -ErrorAction SilentlyContinue

  # Referências (PowerShell usa crase ` para quebra de linha)
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" reference `
    "$projApp/$SolutionName.$svc.Application.csproj" `
    "$projInfra/$SolutionName.$svc.Infrastructure.csproj" `
    "$projDom/$SolutionName.$svc.Domain.csproj" | Out-Null

  dotnet add "$projApp/$SolutionName.$svc.Application.csproj" reference `
    "$sharedKernel/$SolutionName.BuildingBlocks.SharedKernel.csproj" `
    "$abstractions/$SolutionName.BuildingBlocks.Abstractions.csproj" `
    "$projDom/$SolutionName.$svc.Domain.csproj" | Out-Null

  dotnet add "$projInfra/$SolutionName.$svc.Infrastructure.csproj" reference `
    "$projDom/$SolutionName.$svc.Domain.csproj" `
    "$abstractions/$SolutionName.BuildingBlocks.Abstractions.csproj" | Out-Null

  dotnet add "$projDom/$SolutionName.$svc.Domain.csproj" reference `
    "$sharedKernel/$SolutionName.BuildingBlocks.SharedKernel.csproj" | Out-Null

  # Pacotes
  Add-Common-Packages "$projApp/$SolutionName.$svc.Application.csproj"

  dotnet add "$projInfra/$SolutionName.$svc.Infrastructure.csproj" package Dapper | Out-Null
  dotnet add "$projInfra/$SolutionName.$svc.Infrastructure.csproj" package Npgsql | Out-Null
  dotnet add "$projInfra/$SolutionName.$svc.Infrastructure.csproj" package Microsoft.Extensions.Configuration.Abstractions | Out-Null
  dotnet add "$projInfra/$SolutionName.$svc.Infrastructure.csproj" package Microsoft.Extensions.Options.ConfigurationExtensions | Out-Null

  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package MediatR | Out-Null
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package FluentValidation | Out-Null
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package Swashbuckle.AspNetCore | Out-Null
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package OpenTelemetry.Exporter.Console | Out-Null
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package OpenTelemetry.Exporter.Otlp | Out-Null
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package OpenTelemetry.Extensions.Hosting | Out-Null
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package Microsoft.AspNetCore.Authentication.JwtBearer | Out-Null

  # Adicionar à solução
  dotnet sln add `
    "$projApi/$SolutionName.$svc.Api.csproj" `
    "$projApp/$SolutionName.$svc.Application.csproj" `
    "$projDom/$SolutionName.$svc.Domain.csproj" `
    "$projInfra/$SolutionName.$svc.Infrastructure.csproj" | Out-Null
}

# 6) Gateway YARP
Dotnet-New webapi $gateway "$SolutionName.Gateway"
dotnet add "$gateway/$SolutionName.Gateway.csproj" package Yarp.ReverseProxy | Out-Null

dotnet sln add "$gateway/$SolutionName.Gateway.csproj" | Out-Null

# 7) Aspire AppHost (console com bootstrap)
Dotnet-New console $apphost "$SolutionName.AppHost"
dotnet sln add "$apphost/$SolutionName.AppHost.csproj" | Out-Null

# 8) Frontends (Blazor WASM + MudBlazor + microfronts como RCL)
$appShell = Join-Path $fronts "app-shell"
Dotnet-New blazorwasm $appShell "$SolutionName.AppShell"
dotnet add "$appShell/$SolutionName.AppShell.csproj" package MudBlazor | Out-Null

dotnet sln add "$appShell/$SolutionName.AppShell.csproj" | Out-Null

$microfronts = @("mf-patient","mf-appointment","mf-ehr","mf-billing","ui-kit")
foreach ($mf in $microfronts) {
  $mfPath = Join-Path $fronts $mf
  Dotnet-New razorclasslib $mfPath "$SolutionName.$($mf -replace '-', '.')"
  dotnet sln add "$mfPath/$SolutionName.$($mf -replace '-', '.').csproj" | Out-Null
  dotnet add "$appShell/$SolutionName.AppShell.csproj" reference "$mfPath/$SolutionName.$($mf -replace '-', '.').csproj" | Out-Null
}

# 9) Arquivos base do SharedKernel/Abstractions
$sharedKernelCode = @"
namespace $Company.$SolutionName.BuildingBlocks.SharedKernel;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; }
    protected Entity(TId id) => Id = id;
}

public readonly record struct Result<T>(bool Success, T? Value, string? Error)
{
    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}
"@
Write-File (Join-Path $sharedKernel "Class1.cs") $sharedKernelCode

$abstractionsCode = @"
namespace $Company.$SolutionName.BuildingBlocks.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IDbConnectionFactory
{
    System.Data.IDbConnection Create();
}
"@
Write-File (Join-Path $abstractions "Class1.cs") $abstractionsCode

# 10) Implementação exemplo: Patients (Domain, Application, Infrastructure, Api)
$patientsRoot = Join-Path $services "Patients"
$patientsDomain = Join-Path $patientsRoot "$SolutionName.Patients.Domain"
$patientsApp = Join-Path $patientsRoot "$SolutionName.Patients.Application"
$patientsInfra = Join-Path $patientsRoot "$SolutionName.Patients.Infrastructure"
$patientsApi = Join-Path $patientsRoot "$SolutionName.Patients.Api"

$patientDomainCode = @"
namespace $Company.$SolutionName.Patients.Domain;
using $Company.$SolutionName.BuildingBlocks.SharedKernel;

public readonly record struct PatientId(Guid Value)
{
    public static PatientId New() => new(Guid.NewGuid());
}

public sealed class Patient : Entity<PatientId>
{
    public string FullName { get; private set; }
    public string Document { get; private set; }
    public DateOnly BirthDate { get; private set; }

    private Patient() : base(new PatientId(Guid.Empty)) { FullName = Document = string.Empty; BirthDate = default; }

    public Patient(PatientId id, string fullName, string document, DateOnly birthDate) : base(id)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName obrigatório");
        if (string.IsNullOrWhiteSpace(document)) throw new ArgumentException("Document obrigatório");
        FullName = fullName; Document = document; BirthDate = birthdate;
    }
}
"@
Write-File (Join-Path $patientsDomain "Patient.cs") $patientDomainCode

$patientsAppCode = @"
namespace $Company.$SolutionName.Patients.Application;
using MediatR;
using $Company.$SolutionName.BuildingBlocks.SharedKernel;
using $Company.$SolutionName.Patients.Domain;

public record RegisterPatientCommand(string FullName, string Document, DateOnly BirthDate) : IRequest<Result<Guid>>;

public interface IPatientRepository
{
    Task AddAsync(Patient entity, CancellationToken ct);
    Task<Patient?> GetAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Patient>> ListAsync(int skip, int take, CancellationToken ct);
}

public sealed class RegisterPatientHandler(IPatientRepository repo, $Company.$SolutionName.BuildingBlocks.Abstractions.IUnitOfWork uow) : IRequestHandler<RegisterPatientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterPatientCommand request, CancellationToken ct)
    {
        var entity = new Patient(PatientId.New(), request.FullName, request.Document, request.BirthDate);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Ok(entity.Id.Value);
    }
}
"@
Write-File (Join-Path $patientsApp "Application.cs") $patientsAppCode

$patientsInfraCode = @"
namespace $Company.$SolutionName.Patients.Infrastructure;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using $Company.$SolutionName.BuildingBlocks.Abstractions;
using $Company.$SolutionName.Patients.Application;
using $Company.$SolutionName.Patients.Domain;

public sealed class NpgsqlConnectionFactory(IConfiguration cfg) : IDbConnectionFactory
{
    public IDbConnection Create()
    {
        var cs = cfg.GetConnectionString("PatientsDb") ?? throw new InvalidOperationException("Missing PatientsDb connection string");
        return new Npgsql.NpgsqlConnection(cs);
    }
}

public sealed class UnitOfWork(IDbConnectionFactory f) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0); // Dapper: transacione por operação conforme necessidade.
}

public sealed class PatientRepository(IDbConnectionFactory f) : IPatientRepository
{
    public async Task AddAsync(Patient entity, CancellationToken ct)
    {
        const string sql = "INSERT INTO patients.patient(id, full_name, document, birth_date) VALUES (@Id, @FullName, @Document, @BirthDate);";
        using var con = f.Create();
        await con.ExecuteAsync(new CommandDefinition(sql, new { Id = entity.Id.Value, entity.FullName, entity.Document, BirthDate = entity.BirthDate }, cancellationToken: ct));
    }

    public async Task<Patient?> GetAsync(Guid id, CancellationToken ct)
    {
        const string sql = "SELECT id, full_name, document, birth_date FROM patients.patient WHERE id = @Id";
        using var con = f.Create();
        var row = await con.QuerySingleOrDefaultAsync(sql, new { Id = id });
        if (row is null) return null;
        return new Patient(new PatientId((Guid)row.id), (string)row.full_name, (string)row.document, DateOnly.FromDateTime((DateTime)row.birth_date));
    }

    public async Task<IEnumerable<Patient>> ListAsync(int skip, int take, CancellationToken ct)
    {
        const string sql = "SELECT id, full_name, document, birth_date FROM patients.patient ORDER BY full_name OFFSET @Skip LIMIT @Take";
        using var con = f.Create();
        var rows = await con.QueryAsync(sql, new { Skip = skip, Take = take });
        return rows.Select(r => new Patient(new PatientId((Guid)r.id), (string)r.full_name, (string)r.document, DateOnly.FromDateTime((DateTime)r.birth_date)));
    }
}

public static class PatientsInfraRegistration
{
    public static IServiceCollection AddPatientsInfra(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        return services;
    }
}
"@
Write-File (Join-Path $patientsInfra "Infrastructure.cs") $patientsInfraCode

$patientsApiProgram = @"
using MediatR;
using $Company.$SolutionName.Patients.Application;
using $Company.$SolutionName.Patients.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

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
"@
Write-File (Join-Path $patientsApi "Program.cs") $patientsApiProgram

$appsettingsPatients = @"
{
  ""ConnectionStrings"": {
    ""PatientsDb"": ""Host=localhost;Port=5432;Database=patientsdb;Username=postgres;Password=postgres""
  },
  ""Auth"": {
    ""Authority"": ""https://YOUR-PROJECT.supabase.co/auth/v1"",
    ""Issuer"": ""https://YOUR-PROJECT.supabase.co/auth/v1""
  }
}
"@
Write-File (Join-Path $patientsApi "appsettings.json") $appsettingsPatients

# 11) Gateway Program.cs e config YARP (in-memory)
$gatewayProgram = @"
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy().LoadFromMemory(new[]
{
    new Yarp.ReverseProxy.Configuration.RouteConfig()
    {
        RouteId = "patients",
        ClusterId = "patients",
        Match = new(){ Path = "/patients/{**catch-all}" }
    }
}, new[]
{
    new Yarp.ReverseProxy.Configuration.ClusterConfig()
    {
        ClusterId = "patients",
        Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
        {
            ["d1"] = new() { Address = "http://localhost:5280" }
        }
    }
});

var app = builder.Build();
app.MapReverseProxy();
app.Run();
"@
Write-File (Join-Path $gateway "Program.cs") $gatewayProgram

# 12) AppHost (mensagem de bootstrap)
$appHostProgram = @"
Console.WriteLine("Starting $SolutionName AppHost (dev bootstrap)...");
Console.WriteLine("- Ensure Postgres and Redis are running.");
Console.WriteLine("- Start services and gateway with 'dotnet run' in each folder or attach your orchestrator of choice.");
"@
Write-File (Join-Path $apphost "Program.cs") $appHostProgram

# 13) Frontend AppShell bootstrap (MudBlazor + layout mínimo)
$appShellProgram = @"
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using $Company.$SolutionName.AppShell;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddMudServices();
await builder.Build().RunAsync();
"@
Write-File (Join-Path $appShell "Program.cs") $appShellProgram

$appShellAppRazor = @"
<App>
  <Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
      <RouteView RouteData="routeData" />
    </Found>
    <NotFound>
      <p>Rota não encontrada.</p>
    </NotFound>
  </Router>
</App>
"@
Write-File (Join-Path $appShell "App.razor") $appShellAppRazor

# 14) Scripts SQL de bootstrap (schema patients)
$sqlDir = Join-Path $root "database"
Ensure-Dir $sqlDir
$patientsSql = @"
CREATE SCHEMA IF NOT EXISTS patients;
CREATE TABLE IF NOT EXISTS patients.patient (
  id uuid PRIMARY KEY,
  full_name varchar(160) NOT NULL,
  document varchar(32) NOT NULL,
  birth_date date NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_patient_full_name ON patients.patient(full_name);
"@
Write-File (Join-Path $sqlDir "01_patients.sql") $patientsSql

# 15) README
$readme = @"
# $SolutionName (Blueprint Clean Architecture + Dapper + .NET 9 + Blazor/MudBlazor + YARP + Supabase)

## Como usar
1. Execute o script: `pwsh ./healthtech-bootstrap.ps1 -SolutionName $SolutionName`
2. Suba o Postgres e rode os scripts em `database/`.
3. Ajuste `appsettings.json` com as strings do Supabase e Auth JWKS/Issuer.
4. Rode as APIs e o Gateway:
   - `dotnet run --project src/services/Patients/$SolutionName.Patients.Api`
   - `dotnet run --project src/gateway/$SolutionName.Gateway`
5. Rode o AppShell (Blazor WASM):
   - `dotnet run --project src/frontends/app-shell/$SolutionName.AppShell`

## Próximos passos
- Criar endpoints adicionais, testes e observabilidade.
- Adicionar Redis, OTEL Collector e orquestração real com .NET Aspire.
- Incluir microfronts com rotas.
"@
Write-File (Join-Path $root "README.md") $readme

Write-Host "`n✅ Blueprint criado com sucesso! Abra a solução '$SolutionName.sln' e siga o README." -ForegroundColor Green

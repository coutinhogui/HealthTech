using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// ===== Recursos de infraestrutura =====

// ConnectionStrings:schedulingdb sera repassada ao Scheduling
var schedulingDb = builder.AddConnectionString("PatientsDb");

// ===== Projetos (suas APIs e gateway) =====

// Patients API
var patientsApi = builder.AddProject<Projects.HealthTech_Patients_Api>("patients-api")
                         .WithReference(schedulingDb);
//                         .WithReference(redis)        // injeta services__redis__...
//                         .WithHttpEndpoint(env: "ASPNETCORE_URLS"); // expõe endpoint http

// Appointments API (se existir, mesmo padrão)
// var appointmentsDb = pg.AddDatabase("appointmentsdb");
// var appointmentsApi = builder.AddProject<Projects.HealthTech_Appointments_Api>("appointments-api")
//                              .WithReference(appointmentsDb)
//                              .WithReference(redis)
//                              .WithHttpEndpoint(env: "ASPNETCORE_URLS");

// Gateway (YARP) – vamos usar service discovery para localizar as APIs
var gateway = builder.AddProject<Projects.HealthTech_Gateway>("gateway")
                     .WithReference(patientsApi);

// (Opcional) Frontend Blazor WASM como projeto .NET (dev server)
var appShell = builder.AddProject<Projects.HealthTech_AppShell>("app-shell")
                      .WithReference(gateway);


// Dashboard do Aspire abre automaticamente ao rodar o AppHost
builder.Build().Run();

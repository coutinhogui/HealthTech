using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var platformDb = builder.AddConnectionString("PatientsDb");

var patientsApi = builder.AddProject<Projects.HealthTech_Patients_Api>("patients-api")
    .WithReference(platformDb);

var identityApi = builder.AddProject<Projects.HealthTech_Identity_Api>("identity-api");

var appointmentsApi = builder.AddProject<Projects.HealthTech_Appointments_Api>("appointments-api")
    .WithReference(platformDb);

var gateway = builder.AddProject<Projects.HealthTech_Gateway>("gateway")
    .WithReference(patientsApi)
    .WithReference(identityApi)
    .WithReference(appointmentsApi);

builder.AddProject<Projects.HealthTech_Front>("front")
    .WithReference(gateway)
    .WithExternalHttpEndpoints();

builder.Build().Run();

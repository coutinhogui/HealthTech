var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.HealthTech_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

var api = builder.AddProject<Projects.HealthTech_ApiService>("apiservice")
    .WithEnvironment("Supabase__Url",   Environment.GetEnvironmentVariable("Supabase__Url") ?? "")
    .WithEnvironment("Supabase__ServiceKey", Environment.GetEnvironmentVariable("Supabase__ServiceKey") ?? "");

builder.AddProject<Projects.HealthTech_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.ECommerce_ApiService>("apiservice");

// builder.AddProject<Projects.ECommerce_Web>("webfrontend")
//     .WithExternalHttpEndpoints()
//     .WithReference(apiService);

builder.Build().Run();

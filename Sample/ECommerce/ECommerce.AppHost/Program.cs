var builder =  DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.ECommerce_ApiService>("apiservice");

var postgresdb = builder.AddPostgres("carts-db")
    .WithEnvironment("POSTGRES_DB", "carts")
    .AddDatabase("carts");

var shoppingCarts = builder.AddProject<Projects.Carts_Api>("api-shopping-carts")
    .WithSwaggerUI()
    .WithReference(postgresdb);

// builder.AddProject<Projects.ECommerce_Web>("webfrontend")
//     .WithExternalHttpEndpoints()
//     .WithReference(apiService);

builder.Build().Run();

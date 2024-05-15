var builder =  DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.ECommerce_ApiService>("apiservice");

var cartsDB = builder.AddPostgres("carts-db")
    .WithEnvironment("POSTGRES_DB", "carts")
    .WithPgAdmin()
    .AddDatabase("carts");

var ordersDB = builder.AddPostgres("orders-db")
    .WithEnvironment("POSTGRES_DB", "orders")
    .WithPgAdmin()
    .AddDatabase("orders");

var shipmentsDB = builder.AddPostgres("shipments-db")
    .WithEnvironment("POSTGRES_DB", "shipments")
    .WithPgAdmin()
    .AddDatabase("shipments");

var shoppingCarts = builder.AddProject<Projects.Carts_Api>("api-shopping-carts")
    .WithSwaggerUI()
    .WithReference(cartsDB);

var orders = builder.AddProject<Projects.Orders_Api>("api-orders")
    .WithSwaggerUI()
    .WithReference(ordersDB);

// builder.AddProject<Projects.ECommerce_Web>("webfrontend")
//     .WithExternalHttpEndpoints()
//     .WithReference(apiService);

builder.Build().Run();

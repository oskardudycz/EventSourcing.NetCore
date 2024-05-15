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

var kafka = builder.AddKafka("kafka")
    .WithEnvironment("KAFKA_AUTO_CREATE_TOPICS_ENABLE", "true")
    .WithEnvironment("AUTO_CREATE_TOPICS_ENABLE", "true")
    .WithEnvironment("KAFKA_TOPIC_CREATE_BACKOFF_MS", "10000")
    .WithEnvironment("TOPIC_CREATE_BACKOFF_MS", "10000");

var shoppingCarts = builder.AddProject<Projects.Carts_Api>("api-shopping-carts")
    .WithSwaggerUI()
    .WithReference(cartsDB)
    .WithReference(kafka);

var orders = builder.AddProject<Projects.Orders_Api>("api-orders")
    .WithSwaggerUI()
    .WithEnvironment("kafka_topics", "['Carts', 'Payments', 'Shipments']")
    .WithReference(ordersDB)
    .WithReference(kafka);

// builder.AddProject<Projects.ECommerce_Web>("webfrontend")
//     .WithExternalHttpEndpoints()
//     .WithReference(apiService);

builder.Build().Run();

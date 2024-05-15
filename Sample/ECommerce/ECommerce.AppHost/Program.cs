var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.ECommerce_ApiService>("apiservice");

var postgres = builder.AddPostgres("postgres-db", password: builder.CreateStablePassword("sqlpassword", minLower: 1, minUpper: 1, minNumeric: 1, minSpecial:0))
    .WithEnvironment("POSTGRES_MULTIPLE_DATABASES", "carts,payments,orders,shipments")
    .WithPgAdmin()
    .WithBindMount(
        "./Containers/Postgres/Init",
        "/docker-entrypoint-initdb.d")
    .WithDataVolume();

var cartsDb = postgres
    .AddDatabase("carts");

var ordersDb = postgres.AddDatabase("orders");

var paymentsDB = postgres.AddDatabase("payments");
//
// var ordersDB = builder.AddPostgres("orders-db")
//     .WithEnvironment("POSTGRES_DB", "orders")
//     .WithPgAdmin()
//     .WithDataVolume("orders")
//     .AddDatabase("orders");
//
// var paymentsDB = builder.AddPostgres("payments-db")
//     .WithEnvironment("POSTGRES_DB", "payments")
//     //.WithHealthCheck()
//     .WithPgAdmin()
//     .WithDataVolume("payments")
//     .AddDatabase("payments");

// var shipmentsDB = builder.AddPostgres("shipments-db")
//     .WithEnvironment("POSTGRES_DB", "shipments")
//     .WithHealthCheck()
//     .WithPgAdmin()
//     .WithDataVolume()
//     .AddDatabase("shipments");

var kafka = builder.AddKafka("kafka")
    .WithDataVolume()
    .WithHealthCheck()
    .WithEnvironment("KAFKA_AUTO_CREATE_TOPICS_ENABLE", "true")
    .WithEnvironment("AUTO_CREATE_TOPICS_ENABLE", "true")
    .WithEnvironment("KAFKA_TOPIC_CREATE_BACKOFF_MS", "10000")
    .WithEnvironment("TOPIC_CREATE_BACKOFF_MS", "10000");

var shoppingCarts = builder.AddProject<Projects.Carts_Api>("api-shopping-carts")
    .WithSwaggerUI()
    .WithReference(cartsDb)
    .WithReference(kafka)
    .WaitFor(cartsDb)
    .WaitFor(kafka);

var payments = builder.AddProject<Projects.Payments_Api>("paymentsapi")
    .WithSwaggerUI()
    //.WithEnvironment("kafka_topics", "['Carts', 'Payments', 'Shipments']")
    .WithReference(paymentsDB)
    .WithReference(kafka)
    .WaitFor(paymentsDB)
    .WaitFor(kafka);

var orders = builder.AddProject<Projects.Orders_Api>("api-orders")
    .WithSwaggerUI()
    //.WithEnvironment("kafka_topics", "['Carts', 'Payments', 'Shipments']")
    .WithReference(ordersDb)
    .WithReference(kafka)
    .WithReference(payments)
    .WaitFor(ordersDb)
    .WaitFor(kafka);
//.WaitForCompletion(payments);

// builder.AddProject<Projects.ECommerce_Web>("webfrontend")
//     .WithExternalHttpEndpoints()
//     .WithReference(apiService);

builder.Build().Run();

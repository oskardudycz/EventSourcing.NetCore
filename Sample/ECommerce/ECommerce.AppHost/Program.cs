var builder = DistributedApplication.CreateBuilder(args);

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

var kafka = builder.AddKafka("kafka")
    .WithDataVolume()
    .WithHealthCheck()
    .WithEnvironment("KAFKA_AUTO_CREATE_TOPICS_ENABLE", "true")
    .WithEnvironment("AUTO_CREATE_TOPICS_ENABLE", "true")
    .WithEnvironment("KAFKA_TOPIC_CREATE_BACKOFF_MS", "10000")
    .WithEnvironment("TOPIC_CREATE_BACKOFF_MS", "10000");

var shoppingCarts = builder.AddProject<Projects.Carts_Api>("api-shopping-carts")
    .WithReference(cartsDb)
    .WithReference(kafka)
    .WaitFor(cartsDb)
    .WaitFor(kafka);

var payments = builder.AddProject<Projects.Payments_Api>("paymentsapi")
    //.WithEnvironment("kafka_topics", "['Carts', 'Payments', 'Shipments']")
    .WithReference(paymentsDB)
    .WithReference(kafka)
    .WaitFor(paymentsDB)
    .WaitFor(kafka);

var orders = builder.AddProject<Projects.Orders_Api>("api-orders")
    //.WithEnvironment("kafka_topics", "['Carts', 'Payments', 'Shipments']")
    .WithReference(ordersDb)
    .WithReference(kafka)
    .WithReference(payments)
    .WaitFor(ordersDb)
    .WaitFor(kafka);
//.WaitForCompletion(payments);

builder.Build().Run();

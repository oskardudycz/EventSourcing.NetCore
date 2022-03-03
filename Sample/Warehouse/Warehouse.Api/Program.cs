using Core.WebApi.Middlewares.ExceptionHandling;
using Warehouse;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder
            .ConfigureServices(services =>
            {
                services.AddRouting()
                    .AddWarehouseServices();
            })
            .Configure(app =>
            {
                app.UseExceptionHandlingMiddleware()
                    .UseRouting()
                    .UseEndpoints(endpoints =>
                    {
                        endpoints.UseWarehouseEndpoints();
                    })
                    .ConfigureWarehouse();
            });
    })
    .Build();
builder.Run();

using Core.WebApi.Middlewares.ExceptionHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                app.UseMiddleware(typeof(ExceptionHandlingMiddleware))
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

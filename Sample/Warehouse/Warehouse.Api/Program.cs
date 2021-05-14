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
                    .AddCors()
                    .AddAuthorization()
                    .AddWarehouseServices();
            })
            .Configure(app =>
            {
                app.UseHttpsRedirection()
                    .UseMiddleware(typeof(ExceptionHandlingMiddleware))
                    .UseRouting()
                    .UseAuthorization()
                    .UseEndpoints(endpoints =>
                    {
                        endpoints.UseWarehouseEndpoints();
                    });
            });
    })
    .Build();
builder.Run();

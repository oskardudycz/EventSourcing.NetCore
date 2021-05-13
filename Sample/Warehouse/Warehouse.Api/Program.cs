using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Warehouse;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder
            .ConfigureServices(services =>
            {
                services.AddMvcCore()
                    .AddApiExplorer()
                    .AddAuthorization()
                    .AddCors()
                    .AddDataAnnotations()
                    .AddFormatterMappings();

                services
                    .AddWarehouseServices()
                    .AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new OpenApiInfo {Title = "Warehouse.Api", Version = "v1"});
                    });
            })
            .Configure(app =>
            {
                app.UseSwagger()
                    .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CashRegisters.Api v1"))
                    .UseHttpsRedirection()
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

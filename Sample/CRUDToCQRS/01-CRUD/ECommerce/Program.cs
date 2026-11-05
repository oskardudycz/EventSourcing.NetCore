using ECommerce.Repositories;
using ECommerce.Services;
using ECommerce.Storage;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<ECommerceDbContext>()
    .AddScoped<ProductRepository>()
    .AddScoped<ProductService>()
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce", Version = "V1" });
    })
    .AddControllers();

var app = builder.Build();

app.UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    })
    .UseSwagger()
    .UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce V1");
        c.RoutePrefix = string.Empty;
    });

app.Run();

public partial class Program;

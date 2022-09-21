using System.Net;
using ECommerce.Domain;
using ECommerce.Domain.Products;
using ECommerce.Domain.Products.Entity;
using ECommerce.Domain.Products.Repositories;
using ECommerce.Domain.Products.Services;
using ECommerce.Domain.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddECommerce()
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

public partial class Program
{
}

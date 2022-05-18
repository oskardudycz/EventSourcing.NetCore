using System.Net;
using Core.WebApi.Middlewares.ExceptionHandling;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Products;
using Warehouse.Api.Storage;
using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddDbContext<WarehouseDBContext>(
        options => options.UseNpgsql("name=ConnectionStrings:WarehouseDB"))
    .AddTransient<IProductService, ProductService>()
    .AddTransient<IProductsQueryService, ProductsQueryService>();

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger()
        .UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    await using var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDBContext>();
    await dbContext.Database.MigrateAsync();
}

// Get Products
app.MapGet(
        "/api/products",
        (
            IProductsQueryService productService,
            string? filter,
            int? page,
            int? pageSize,
            CancellationToken ct
        ) => productService.GetAll(filter, page, pageSize, ct))
    .Produces((int)HttpStatusCode.BadRequest);;


// Get Product Details by Id
app.MapGet("/api/products/{id:guid}",
        async (IProductsQueryService productService, Guid id, CancellationToken ct) =>
        {
            var product = await productService.GetById(id, ct);

            return product == null ? NotFound() : Ok(product);
        })
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound);


// Register new product
app.MapPost(
        "api/products/",
        async (IProductService productService, Product product, CancellationToken ct) =>
        {
            product.Id = Guid.NewGuid();

            await productService.Add(product, ct);

            return Created($"/api/products/{product.Id}", product.Id);
        }
    )
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound);

app.Run();




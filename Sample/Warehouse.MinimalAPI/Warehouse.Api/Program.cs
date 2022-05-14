using System.Net;
using Core.WebApi.Middlewares.ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Core.Commands;
using Warehouse.Api.Core.Queries;
using Warehouse.Api.Products;
using Warehouse.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddDbContext<WarehouseDBContext>(
        options => options.UseNpgsql("name=ConnectionStrings:WarehouseDB"))
    .AddProductServices();

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
                [FromServices] QueryHandler<GetProducts, IReadOnlyList<ProductListItem>> getProducts,
                string? filter,
                int? page,
                int? pageSize,
                CancellationToken ct
            ) =>
            getProducts(GetProducts.With(filter, page, pageSize), ct)
    )
    .Produces((int)HttpStatusCode.BadRequest);


// Get Product Details by Id
app.MapGet(
        "/api/products/{id:guid}",
        async (
                [FromServices] QueryHandler<GetProductDetails, ProductDetails?> getProductById,
                Guid id,
                CancellationToken ct
            ) =>
            await getProductById(GetProductDetails.With(id), ct)
                is { } product
                ? Results.Ok(product)
                : Results.NotFound()
    )
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound);


// Register new product
app.MapPost(
        "api/products/",
        async (
            [FromServices] CommandHandler<RegisterProduct> registerProduct,
            RegisterProductRequest request,
            CancellationToken ct
        ) =>
        {
            var productId = Guid.NewGuid();
            var (sku, name, description) = request;

            await registerProduct(
                RegisterProduct.With(productId, sku, name, description),
                ct);

            return Results.Created($"/api/products/{productId}", productId);
        }
    )
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound);

app.Run();

public record RegisterProductRequest(
    string? SKU,
    string? Name,
    string? Description
);

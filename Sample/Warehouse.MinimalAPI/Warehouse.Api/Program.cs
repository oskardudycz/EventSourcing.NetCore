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
app.MapGet("/api/products", HandleGetProducts)
    .Produces((int)HttpStatusCode.BadRequest);

ValueTask<IReadOnlyList<ProductListItem>> HandleGetProducts(
    [FromServices] QueryHandler<GetProducts, IReadOnlyList<ProductListItem>> getProducts,
    string? filter,
    int? page,
    int? pageSize,
    CancellationToken ct
) =>
    getProducts(GetProducts.With(filter, page, pageSize), ct);


// Get Product Details by Id
app.MapGet("/api/products/{id}", HandleGetProductDetails)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound);

async Task<IResult> HandleGetProductDetails(
    [FromServices] QueryHandler<GetProductDetails, ProductDetails?> getProductById,
    Guid productId,
    CancellationToken ct
) =>
    await getProductById(GetProductDetails.With(productId), ct)
        is { } product
        ? Results.Ok(product)
        : Results.NotFound();

// Register new product
app.MapPost("api/products/",HandleRegisterProduct)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound);

async Task<IResult> HandleRegisterProduct(
    [FromServices] CommandHandler<RegisterProduct> registerProduct,
    RegisterProductRequest request,
    CancellationToken ct
)
{
    var productId = Guid.NewGuid();
    var (sku, name, description) = request;

    await registerProduct(
        RegisterProduct.With(productId, sku, name, description),
        ct);

    return Results.Created($"/api/products/{productId}", productId);
}

app.Run();

public record RegisterProductRequest(
    string? SKU,
    string? Name,
    string? Description
);

public partial class Program { }

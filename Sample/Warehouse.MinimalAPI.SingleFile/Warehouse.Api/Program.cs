using Core.WebApi.Middlewares.ExceptionHandling;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Storage;
using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddDbContext<WarehouseDBContext>(
        options => options.UseNpgsql("name=ConnectionStrings:WarehouseDB"));

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
        async (
            WarehouseDBContext dbContext,
            string? filter,
            int? page,
            int? pageSize,
            CancellationToken ct
        ) =>
        {
            var take = pageSize ?? 0;
            var pageNumber = page ?? 1;

            var products = dbContext.Set<Product>();

            var filteredProducts = string.IsNullOrEmpty(filter)
                ? products
                : products
                    .Where(p =>
                        p.Sku.Contains(filter) ||
                        p.Name.Contains(filter) ||
                        p.Description!.Contains(filter)
                    );

            return await filteredProducts
                .Skip(take * (pageNumber - 1))
                .Take(take)
                .ToListAsync(ct);
        }
    );


// Get Product Details by Id
app.MapGet("/api/products/{id:guid}",
        async (WarehouseDBContext dbContext, Guid id, CancellationToken ct) =>
        {
            var products = dbContext.Set<Product>();

            var product = await products.SingleOrDefaultAsync(p => p.Id == id, ct);

            return product == null ? NotFound() : Ok(product);
        });


// Register new product
app.MapPost(
        "api/products/",
        async (
            WarehouseDBContext dbContext,
            Product product,
            CancellationToken ct
        ) =>
        {
            var productId = Guid.NewGuid();

            var products = dbContext.Set<Product>();

            if (await products.AnyAsync(p => p.Sku == product.Sku, ct))
            {
                throw new InvalidOperationException(
                    $"Product with SKU `{product.Sku} already exists.");
            }

            await products.AddAsync(product, ct);
            await dbContext.SaveChangesAsync(ct);

            return Created($"/api/products/{productId}", productId);
        }
    );

app.Run();

internal record Product(Guid Id, string Sku, string Name, string? Description);


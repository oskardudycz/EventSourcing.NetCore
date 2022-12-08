using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Core.Commands;
using Warehouse.Api.Core.Entities;
using Warehouse.Api.Core.Queries;
using WarehouseDBContext = Warehouse.Api.Storage.WarehouseDBContext;

namespace Warehouse.Api.Products;

internal static class Configuration
{
    public static IServiceCollection AddProductServices(this IServiceCollection services)
        => services
            .AddQueryable<Product, WarehouseDBContext>()
            .AddCommandHandler<RegisterProduct, HandleRegisterProduct>(s =>
            {
                var dbContext = s.GetRequiredService<WarehouseDBContext>();
                return new HandleRegisterProduct(dbContext.AddAndSave, dbContext.ProductWithSKUExists);
            })
            .AddQueryHandler<GetProducts, IReadOnlyList<ProductListItem>, HandleGetProducts>()
            .AddQueryHandler<GetProductDetails, ProductDetails?, HandleGetProductDetails>();

    public static void SetupProductsModel(this ModelBuilder modelBuilder)
        => modelBuilder.Entity<Product>()
            .OwnsOne(p => p.Sku);

    public static ValueTask<bool> ProductWithSKUExists(this WarehouseDBContext dbContext, SKU productSKU, CancellationToken ct)
        => new (dbContext.Set<Product>().AnyAsync(product => product.Sku.Value == productSKU.Value, ct));
}

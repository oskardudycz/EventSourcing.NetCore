using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Core.Commands;
using Warehouse.Core.Entities;
using Warehouse.Core.Queries;
using Warehouse.Products.GettingProductDetails;
using Warehouse.Products.GettingProducts;
using Warehouse.Products.RegisteringProduct;
using Warehouse.Storage;

namespace Warehouse.Products
{
    internal static class Configuration
    {
        public static IServiceCollection AddProductServices(this IServiceCollection services)
            => services
                .AddCommandHandler<RegisterProduct, HandleRegisterProduct>(s =>
                {
                    var dbContext = s.GetRequiredService<WarehouseDBContext>();
                    return new HandleRegisterProduct(dbContext.AddAndSave, dbContext.ProductWithSKUExists);
                })
                .AddQueryHandler<GetProducts, IReadOnlyList<Product>, HandleGetProducts>(s =>
                {
                    var dbContext = s.GetRequiredService<WarehouseDBContext>();
                    return new HandleGetProducts(dbContext.Set<Product>().AsNoTracking());
                })
                .AddQueryHandler<GetProductDetails, Product?, HandleGetProductDetails>(s =>
                {
                    var dbContext = s.GetRequiredService<WarehouseDBContext>();
                    return new HandleGetProductDetails(dbContext.Set<Product>().AsNoTracking());
                });


        public static IEndpointRouteBuilder UseProductsEndpoints(this IEndpointRouteBuilder endpoints) =>
            endpoints
                .UseRegisterProductEndpoint()
                .UseGetProductsEndpoint()
                .UseGetProductDetailsEndpoint();

        public static void SetupProductsModel(this ModelBuilder modelBuilder)
            => modelBuilder.Entity<Product>()
                .OwnsOne(p => p.Sku);
    }
}

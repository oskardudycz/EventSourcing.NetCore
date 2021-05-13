using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Warehouse.Core.Extensions;
using Warehouse.Core.Queries;

namespace Warehouse.Products.GettingProducts
{
    public static class Route
    {
        internal static IEndpointRouteBuilder UseGetProductsEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/products", async context =>
            {
                // var dbContext = WarehouseDBContextFactory.Create();
                // var handler = new HandleGetProducts(dbContext.Set<Product>().AsQueryable());

                var filter = context.FromQuery<string?>("filter");
                var page = context.FromQuery<int?>("page");
                var query = GetProducts.Create(filter, page);

                var result = context
                    .SendQuery<GetProducts, IReadOnlyList<Product>>(query);

                await context.OK(result);
            });
            return endpoints;
        }
    }
}

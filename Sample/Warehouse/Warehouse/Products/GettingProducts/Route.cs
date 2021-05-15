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

                var filter = context.FromQuery("filter");
                var page = context.FromQuery<int>("page");
                var pageSize = context.FromQuery<int>("pageSize");

                var query = GetProducts.Create(filter, page, pageSize);

                var result = await context
                    .SendQuery<GetProducts, IReadOnlyList<ProductListItem>>(query);

                await context.OK(result);
            });
            return endpoints;
        }
    }
}

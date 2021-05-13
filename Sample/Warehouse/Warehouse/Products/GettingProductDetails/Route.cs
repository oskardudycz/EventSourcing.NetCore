using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Warehouse.Core.Extensions;
using Warehouse.Core.Queries;

namespace Warehouse.Products.GettingProductDetails
{
    public static class Route
    {
        internal static IEndpointRouteBuilder UseGetProductDetailsEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/products/{id}", async context =>
            {
                // var dbContext = WarehouseDBContextFactory.Create();
                // var handler = new HandleGetProductDetails(dbContext.Set<Product>().AsQueryable());

                var productId = context.FromRoute<Guid>("id");
                var query = GetProductDetails.Create(productId);

                var result = await context
                    .SendQuery<GetProductDetails, Product?>(query);

                await context.OK(result);
            });
            return endpoints;
        }
    }
}

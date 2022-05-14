using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Warehouse.Core.Queries;
using static Microsoft.AspNetCore.Http.Results;

namespace Warehouse.Products.GettingProducts;

public static class Route
{
    internal static IEndpointRouteBuilder UseGetProductsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/products", async (HttpContext context, [FromQuery]string? filter, [FromQuery]int? page, [FromQuery]int? pageSize) =>
        {
            var query = GetProducts.Create(filter, page, pageSize);

            var result = await context
                .SendQuery<GetProducts, IReadOnlyList<ProductListItem>>(query);

            return Ok(result);
        });
        return endpoints;
    }
}

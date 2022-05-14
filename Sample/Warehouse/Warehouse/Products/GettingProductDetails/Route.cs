using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Warehouse.Core.Queries;
using static Microsoft.AspNetCore.Http.Results;

namespace Warehouse.Products.GettingProductDetails;

public static class Route
{
    internal static IEndpointRouteBuilder UseGetProductDetailsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/products/{id:guid}", async (HttpContext context, Guid id) =>
        {
            var query = GetProductDetails.Create(id);

            var result = await context
                .SendQuery<GetProductDetails, ProductDetails?>(query);

            return result != null ? Ok(result) : NotFound();
        });
        return endpoints;
    }
}

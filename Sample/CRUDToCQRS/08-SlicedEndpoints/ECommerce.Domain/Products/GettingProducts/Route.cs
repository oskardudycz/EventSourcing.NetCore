using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using static Microsoft.AspNetCore.Http.Results;

namespace ECommerce.Domain.Products.GettingProducts;

public static class Route
{
    internal static IEndpointRouteBuilder UseGetProductsEndpoint(this IEndpointRouteBuilder endpoints)
    {

        endpoints.MapGet("/api/products/",
            (
                [FromServices] IQueryable<Product> products,
                CancellationToken ct,
                [FromQuery] int? pageNumber,
                [FromQuery] int? pageSize
            ) =>
                products.HandleAsync(new GetProducts(pageNumber ?? 1, pageSize ?? 20), ct)
        );
        return endpoints;
    }
}

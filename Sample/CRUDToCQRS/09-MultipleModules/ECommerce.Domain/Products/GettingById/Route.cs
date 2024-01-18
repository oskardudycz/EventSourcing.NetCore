using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Domain.Products.GettingById;

public static class Route
{
    internal static IEndpointRouteBuilder UseGetProductByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/products/{id:guid}",
            (
                [FromServices] IQueryable<Product> products,
                [FromRoute] Guid id,
                CancellationToken ct
            ) =>
                products.HandleAsync(new GetProductById(id), ct)
        );
        return endpoints;
    }
}

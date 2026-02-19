using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Warehouse.Core.Commands;
using static Microsoft.AspNetCore.Http.Results;

namespace Warehouse.Products.RegisteringProduct;

public record RegisterProductRequest(
    string? SKU,
    string? Name,
    string? Description
);

internal static class Route
{
    internal static IEndpointRouteBuilder UseRegisterProductEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("api/products/", async (HttpContext context, RegisterProductRequest request) =>
        {
            var (sku, name, description) = request;
            var productId = Guid.CreateVersion7();

            var command = RegisterProduct.Create(productId, sku, name, description);

            await context.SendCommand(command);

            return Created($"/api/products/{productId}", productId);
        });


        return endpoints;
    }
}

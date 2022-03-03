using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Warehouse.Core.Commands;
using Warehouse.Core.Extensions;

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
        endpoints.MapPost("api/products/", async context =>
        {
            var (sku, name, description) = await context.FromBody<RegisterProductRequest>();
            var productId = Guid.NewGuid();

            var command = RegisterProduct.Create(productId, sku, name, description);

            await context.SendCommand(command);

            await context.Created(productId);
        });

        return endpoints;
    }
}
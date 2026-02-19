using ECommerce.Domain.Core.Http;
using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Domain.Products.UpdatingProduct;

using static UpdateProductHandler;
using static HttpExtensions;

internal static class Route
{
    internal static IEndpointRouteBuilder UseUpdateProductEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("api/products/{id:guid}",
            async (
                HttpContext context,
                [FromServices] ECommerceDbContext dbContext,
                [FromRoute] Guid id,
                [FromBody] UpdateProductRequest request,
                CancellationToken ct) =>
            {
                var command = request.ToCommand(Guid.CreateVersion7());

                await dbContext.UpdateAndSaveChanges<Product>(
                    command.Id,
                    product => Handle(product, command),
                    ct
                );

                return OkWithLocation(context, $"/api/products/{command.Id}");
            });


        return endpoints;
    }
}

public record UpdateProductRequest(
    string? Name,
    string? Description,
    string? ProducerName,
    string? AdditionalInfo
)
{
    public UpdateProduct ToCommand(Guid id) =>
        new(
            id,
            Name ?? throw new ArgumentNullException(nameof(Name)),
            Description,
            ProducerName ?? throw new ArgumentNullException(nameof(ProducerName)),
            AdditionalInfo
        );
}

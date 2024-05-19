using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Domain.Products.CreatingProduct;

using static CreateProductHandler;
using static Microsoft.AspNetCore.Http.Results;

internal static class Route
{
    internal static IEndpointRouteBuilder UseCreateProductEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("api/products",
            async (
                [FromServices] ECommerceDbContext dbContext,
                [FromBody] CreateProductRequest request,
                CancellationToken ct
            ) =>
            {
                var command = request.ToCommand(Guid.NewGuid());

                await dbContext.AddAndSaveChanges(
                    Handle(command),
                    ct
                );

                return Created($"/api/products/{command.Id}", command.Id);
            });


        return endpoints;
    }
}

public record CreateProductRequest(
    string? Sku,
    string? Name,
    string? Description,
    string? ProducerName,
    string? AdditionalInfo
)
{
    public CreateProduct ToCommand(Guid id) =>
        new(
            id,
            Sku ?? throw new ArgumentNullException(nameof(Sku)),
            Name ?? throw new ArgumentNullException(nameof(Name)),
            Description,
            ProducerName ?? throw new ArgumentNullException(nameof(ProducerName)),
            AdditionalInfo
        );
}

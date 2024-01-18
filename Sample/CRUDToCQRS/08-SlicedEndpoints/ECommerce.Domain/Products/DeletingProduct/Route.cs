using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Domain.Products.DeletingProduct;

using static Results;

internal static class Route
{
    internal static IEndpointRouteBuilder UseDeleteProductByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("api/products/{id:guid}",
            async (
                HttpContext context,
                [FromServices] ECommerceDbContext dbContext,
                [FromRoute] Guid id,
                CancellationToken ct) =>
            {
                await dbContext.DeleteAndSaveChanges<Product>(id, ct);

                return NoContent();
            });


        return endpoints;
    }
}

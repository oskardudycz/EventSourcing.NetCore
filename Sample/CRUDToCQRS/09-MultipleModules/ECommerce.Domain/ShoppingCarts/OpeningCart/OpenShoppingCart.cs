using ECommerce.Domain.Core.Outbox;
using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Domain.ShoppingCarts.OpeningCart;

using static Microsoft.AspNetCore.Http.Results;

public record OpenShoppingCartRequest(
    Guid? ClientId
)
{
    public OpenShoppingCart ToCommand(Guid cartId) =>
        new OpenShoppingCart(
            cartId,
            ClientId ?? throw new ArgumentNullException(nameof(cartId))
        );
}

public record OpenShoppingCart(
    Guid CartId,
    Guid ClientId
);

public record ShoppingCartOpened(
    Guid CartId,
    Guid ClientId
);

internal static class Route
{
    internal static IEndpointRouteBuilder UseCreateProductEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("api/shopping-carts",
            async (
                [FromServices] ECommerceDbContext dbContext,
                [FromServices] IOutbox outbox,
                [FromBody] OpenShoppingCartRequest request,
                CancellationToken ct
            ) =>
            {
                var command = request.ToCommand(Guid.NewGuid());

                var (shoppingCart, @event) = ShoppingCart.Open(command.CartId, command.ClientId);

                await outbox.Enqueue(@event);

                await dbContext.AddAndSaveChanges(
                    shoppingCart,
                    ct
                );

                return Created($"/api/products/{command.CartId}", command.CartId);
            });


        return endpoints;
    }
}

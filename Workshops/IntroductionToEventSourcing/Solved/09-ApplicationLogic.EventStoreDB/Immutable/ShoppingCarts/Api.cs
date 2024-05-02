using ApplicationLogic.EventStoreDB.Core.EventStoreDB;
using ApplicationLogic.EventStoreDB.Immutable.Pricing;
using Core.Validation;
using EventStore.Client;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.TypedResults;
using static System.DateTimeOffset;

namespace ApplicationLogic.EventStoreDB.Immutable.ShoppingCarts;

using static ShoppingCartService;
using static ShoppingCartCommand;

public static class Api
{
    public static WebApplication ConfigureImmutableShoppingCarts(this WebApplication app)
    {
        var clients = app.MapGroup("/api/immutable/clients/{clientId:guid}/");
        var shoppingCarts = clients.MapGroup("shopping-carts");
        var shoppingCart = shoppingCarts.MapGroup("{shoppingCartId:guid}");
        var productItems = shoppingCart.MapGroup("product-items");

        shoppingCarts.MapPost("",
            async (EventStoreClient eventStore,
                Guid clientId,
                CancellationToken ct) =>
            {
                var shoppingCartId = Uuid.NewUuid().ToGuid();

                await eventStore.Add<ShoppingCart>(shoppingCartId,
                    [Handle(new OpenShoppingCart(shoppingCartId, clientId.NotEmpty(), Now))], ct);

                return Created($"/api/immutable/clients/{clientId}/shopping-carts/{shoppingCartId}", shoppingCartId);
            }
        );

        productItems.MapPost("",
            async (
                IProductPriceCalculator pricingCalculator,
                EventStoreClient eventStore,
                Guid shoppingCartId,
                AddProductRequest body,
                CancellationToken ct) =>
            {
                var productItem = body.ProductItem.NotNull().ToProductItem();

                await eventStore.GetAndUpdate(shoppingCartId,
                    state =>
                    [
                        Handle(pricingCalculator,
                            new AddProductItemToShoppingCart(shoppingCartId, productItem, Now),
                            state)
                    ], ct);

                return NoContent();
            }
        );

        productItems.MapDelete("{productId:guid}",
            async (
                EventStoreClient eventStore,
                Guid shoppingCartId,
                [FromRoute] Guid productId,
                [FromQuery] int? quantity,
                [FromQuery] decimal? unitPrice,
                CancellationToken ct) =>
            {
                var productItem = new PricedProductItem(
                    productId.NotEmpty(),
                    quantity.NotNull().Positive(),
                    unitPrice.NotNull().Positive()
                );

                await eventStore.GetAndUpdate(shoppingCartId,
                    state => [Handle(new RemoveProductItemFromShoppingCart(shoppingCartId, productItem, Now), state)],
                    ct);

                return NoContent();
            }
        );

        shoppingCart.MapPost("confirm",
            async (EventStoreClient eventStore,
                Guid shoppingCartId,
                CancellationToken ct) =>
            {
                await eventStore.GetAndUpdate(shoppingCartId,
                    state => [Handle(new ConfirmShoppingCart(shoppingCartId, Now), state)], ct);

                return NoContent();
            }
        );

        shoppingCart.MapDelete("",
            async (EventStoreClient eventStore,
                Guid shoppingCartId,
                CancellationToken ct) =>
            {
                await eventStore.GetAndUpdate(shoppingCartId,
                    state => [Handle(new CancelShoppingCart(shoppingCartId, Now), state)], ct);

                return NoContent();
            }
        );

        shoppingCart.MapGet("",
            async Task<IResult> (
                EventStoreClient eventStore,
                Guid shoppingCartId,
                CancellationToken ct) =>
            {
                var result = await eventStore.GetShoppingCart(shoppingCartId, ct);

                return result is not null ? Ok(result) : NotFound();
            }
        );

        return app;
    }
}

public record ProductItemRequest(
    Guid? ProductId,
    int? Quantity
)
{
    public ProductItem ToProductItem() => new(ProductId.NotEmpty(), Quantity.NotNull().Positive());
}

public record AddProductRequest(
    ProductItemRequest? ProductItem
);

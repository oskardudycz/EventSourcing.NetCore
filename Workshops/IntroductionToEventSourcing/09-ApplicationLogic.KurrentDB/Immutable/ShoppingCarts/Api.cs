using ApplicationLogic.KurrentDB.Core.KurrentDB;
using ApplicationLogic.KurrentDB.Immutable.Pricing;
using Core.Validation;
using KurrentDB.Client;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.TypedResults;
using static System.DateTimeOffset;

namespace ApplicationLogic.KurrentDB.Immutable.ShoppingCarts;

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
            async (KurrentDBClient eventStore,
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
                KurrentDBClient eventStore,
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
                KurrentDBClient eventStore,
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
            async (KurrentDBClient eventStore,
                Guid shoppingCartId,
                CancellationToken ct) =>
            {
                await eventStore.GetAndUpdate(shoppingCartId,
                    state => [Handle(new ConfirmShoppingCart(shoppingCartId, Now), state)], ct);

                return NoContent();
            }
        );

        shoppingCart.MapDelete("",
            async (KurrentDBClient eventStore,
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
                KurrentDBClient eventStore,
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

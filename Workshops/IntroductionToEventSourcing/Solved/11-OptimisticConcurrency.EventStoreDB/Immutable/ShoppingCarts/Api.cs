using Core.Validation;
using EventStore.Client;
using Microsoft.AspNetCore.Mvc;
using OptimisticConcurrency.Core.EventStoreDB;
using OptimisticConcurrency.Core.Http;
using OptimisticConcurrency.Immutable.Pricing;
using static Microsoft.AspNetCore.Http.TypedResults;
using static System.DateTimeOffset;

namespace OptimisticConcurrency.Immutable.ShoppingCarts;

using static ShoppingCartService;
using static ShoppingCartCommand;
using static ETagExtensions;

public static class Api
{
    public static WebApplication ConfigureImmutableShoppingCarts(this WebApplication app)
    {
        var clients = app.MapGroup("/api/immutable/clients/{clientId:guid}/");
        var shoppingCarts = clients.MapGroup("shopping-carts");
        var shoppingCart = shoppingCarts.MapGroup("{shoppingCartId:guid}");
        var productItems = shoppingCart.MapGroup("product-items");

        shoppingCarts.MapPost("",
            async (
                HttpContext context,
                EventStoreClient eventStore,
                Guid clientId,
                CancellationToken ct) =>
            {
                var shoppingCartId = Uuid.NewUuid().ToGuid();

                var nextExpectedRevision = await eventStore.Add<ShoppingCart>(shoppingCartId,
                    [Handle(new OpenShoppingCart(shoppingCartId, clientId.NotEmpty(), Now))], ct);

                context.SetResponseEtag(nextExpectedRevision);

                return Created($"/api/immutable/clients/{clientId}/shopping-carts/{shoppingCartId}", shoppingCartId);
            }
        );

        productItems.MapPost("",
            async (
                HttpContext context,
                IProductPriceCalculator pricingCalculator,
                EventStoreClient eventStore,
                Guid shoppingCartId,
                AddProductRequest body,
                [FromIfMatchHeader] string eTag,
                CancellationToken ct) =>
            {
                var productItem = body.ProductItem.NotNull().ToProductItem();

                var nextExpectedRevision = await eventStore.GetAndUpdate(shoppingCartId, ToExpectedRevision(eTag),
                    state =>
                    [
                        Handle(pricingCalculator,
                            new AddProductItemToShoppingCart(shoppingCartId, productItem, Now),
                            state)
                    ], ct);

                context.SetResponseEtag(nextExpectedRevision);

                return NoContent();
            }
        );

        productItems.MapDelete("{productId:guid}",
            async (
                HttpContext context,
                EventStoreClient eventStore,
                Guid shoppingCartId,
                [FromRoute] Guid productId,
                [FromQuery] int? quantity,
                [FromQuery] decimal? unitPrice,
                [FromIfMatchHeader] string eTag,
                CancellationToken ct) =>
            {
                var productItem = new PricedProductItem(
                    productId.NotEmpty(),
                    quantity.NotNull().Positive(),
                    unitPrice.NotNull().Positive()
                );

                var nextExpectedRevision = await eventStore.GetAndUpdate(shoppingCartId, ToExpectedRevision(eTag),
                    state => [Handle(new RemoveProductItemFromShoppingCart(shoppingCartId, productItem, Now), state)],
                    ct);

                context.SetResponseEtag(nextExpectedRevision);

                return NoContent();
            }
        );

        shoppingCart.MapPost("confirm",
            async (
                HttpContext context,
                EventStoreClient eventStore,
                Guid shoppingCartId,
                [FromIfMatchHeader] string eTag,
                CancellationToken ct) =>
            {
                var nextExpectedRevision = await eventStore.GetAndUpdate(shoppingCartId, ToExpectedRevision(eTag),
                    state => [Handle(new ConfirmShoppingCart(shoppingCartId, Now), state)], ct);

                context.SetResponseEtag(nextExpectedRevision);

                return NoContent();
            }
        );

        shoppingCart.MapDelete("",
            async (
                HttpContext context,
                EventStoreClient eventStore,
                Guid shoppingCartId,
                [FromIfMatchHeader] string eTag,
                CancellationToken ct) =>
            {
                var nextExpectedRevision = await eventStore.GetAndUpdate(shoppingCartId, ToExpectedRevision(eTag),
                    state => [Handle(new CancelShoppingCart(shoppingCartId, Now), state)], ct);

                context.SetResponseEtag(nextExpectedRevision);

                return NoContent();
            }
        );

        shoppingCart.MapGet("",
            async Task<IResult> (
                HttpContext context,
                EventStoreClient eventStore,
                Guid shoppingCartId,
                CancellationToken ct) =>
            {
                var (result, revision) = await eventStore.GetShoppingCart(shoppingCartId, ct);

                context.SetResponseEtag(revision);

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

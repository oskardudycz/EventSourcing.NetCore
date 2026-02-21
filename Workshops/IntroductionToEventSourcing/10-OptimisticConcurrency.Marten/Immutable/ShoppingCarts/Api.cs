using Core.Validation;
using Marten;
using Microsoft.AspNetCore.Mvc;
using OptimisticConcurrency.Core.Http;
using OptimisticConcurrency.Core.Marten;
using OptimisticConcurrency.Immutable.Pricing;
using static Microsoft.AspNetCore.Http.TypedResults;
using static System.DateTimeOffset;

namespace OptimisticConcurrency.Immutable.ShoppingCarts;

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
            async (
                HttpContext context,
                IDocumentSession session,
                Guid clientId,
                CancellationToken ct) =>
            {
                var shoppingCartId = Guid.CreateVersion7();

                await session.Add<ShoppingCart>(shoppingCartId,
                    [Handle(new OpenShoppingCart(shoppingCartId, clientId.NotEmpty(), Now))], ct);

                return Created($"/api/immutable/clients/{clientId}/shopping-carts/{shoppingCartId}", shoppingCartId);
            }
        );

        productItems.MapPost("",
            async (
                HttpContext context,
                IProductPriceCalculator pricingCalculator,
                IDocumentSession session,
                Guid shoppingCartId,
                AddProductRequest body,
                [FromIfMatchHeader] string eTag,
                CancellationToken ct) =>
            {
                var productItem = body.ProductItem.NotNull().ToProductItem();

                await session.GetAndUpdate<ShoppingCart>(shoppingCartId,
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
                HttpContext context,
                IDocumentSession session,
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

                await session.GetAndUpdate<ShoppingCart>(shoppingCartId,
                    state => [Handle(new RemoveProductItemFromShoppingCart(shoppingCartId, productItem, Now), state)],
                    ct);

                return NoContent();
            }
        );

        shoppingCart.MapPost("confirm",
            async (
                HttpContext context,
                IDocumentSession session,
                Guid shoppingCartId,
                [FromIfMatchHeader] string eTag,
                CancellationToken ct) =>
            {
                await session.GetAndUpdate<ShoppingCart>(shoppingCartId,
                    state => [Handle(new ConfirmShoppingCart(shoppingCartId, Now), state)], ct);

                return NoContent();
            }
        );

        shoppingCart.MapDelete("",
            async (
                HttpContext context,
                IDocumentSession session,
                Guid shoppingCartId,
                [FromIfMatchHeader] string eTag,
                CancellationToken ct) =>
            {
                await session.GetAndUpdate<ShoppingCart>(shoppingCartId,
                    state => [Handle(new CancelShoppingCart(shoppingCartId, Now), state)], ct);

                return NoContent();
            }
        );

        shoppingCart.MapGet("",
            async Task<IResult> (
                HttpContext context,
                IQuerySession session,
                Guid shoppingCartId,
                CancellationToken ct) =>
            {
                var result = await session.Events.AggregateStreamAsync<ShoppingCart>(shoppingCartId, token: ct);

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

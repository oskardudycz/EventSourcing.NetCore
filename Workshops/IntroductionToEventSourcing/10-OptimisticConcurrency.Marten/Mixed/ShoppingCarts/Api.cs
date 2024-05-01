using Core.Validation;
using Marten;
using Marten.Schema.Identity;
using Microsoft.AspNetCore.Mvc;
using OptimisticConcurrency.Core.Http;
using OptimisticConcurrency.Core.Marten;
using OptimisticConcurrency.Mixed.Pricing;
using Polly;
using static Microsoft.AspNetCore.Http.TypedResults;
using static System.DateTimeOffset;

namespace OptimisticConcurrency.Mixed.ShoppingCarts;

public static class Api
{
    public static WebApplication ConfigureMixedShoppingCarts(this WebApplication app)
    {
        var clients = app.MapGroup("/api/mixed/clients/{clientId:guid}/");
        var shoppingCarts = clients.MapGroup("shopping-carts");
        var shoppingCart = shoppingCarts.MapGroup("{shoppingCartId:guid}");
        var productItems = shoppingCart.MapGroup("product-items");

        shoppingCarts.MapPost("",
            async (
                HttpContext context,
                IDocumentSession session,
                Guid clientId,
                CancellationToken ct
            ) =>
            {
                var shoppingCartId = CombGuidIdGeneration.NewGuid();

                await session.Add<MixedShoppingCart>(shoppingCartId,
                    [MixedShoppingCart.Open(shoppingCartId, clientId.NotEmpty(), Now).Item1], ct);

                return Created($"/api/mixed/clients/{clientId}/shopping-carts/{shoppingCartId}", shoppingCartId);
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

                await session.GetAndUpdate<MixedShoppingCart>(shoppingCartId,
                    state => [state.AddProduct(pricingCalculator, productItem, Now)], ct);

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
                var productItem = new PricedProductItem
                {
                    ProductId = productId.NotEmpty(),
                    Quantity = quantity.NotNull().Positive(),
                    UnitPrice = unitPrice.NotNull().Positive()
                };

                await session.GetAndUpdate<MixedShoppingCart>(shoppingCartId,
                    state => [state.RemoveProduct(productItem, Now)],
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
                await session.GetAndUpdate<MixedShoppingCart>(shoppingCartId,
                    state => [state.Confirm(Now)], ct);

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
                await session.GetAndUpdate<MixedShoppingCart>(shoppingCartId,
                    state => [state.Cancel(Now)], ct);

                return NoContent();
            }
        );

        shoppingCart.MapGet("",
            async Task<IResult> (
                HttpContext context,
                IQuerySession session,
                Guid shoppingCartId,
                [FromIfMatchHeader] string eTag,
                CancellationToken ct) =>
            {
                var result = await session.Events.AggregateStreamAsync<MixedShoppingCart>(shoppingCartId, token: ct);

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
    public ProductItem ToProductItem() =>
        new() { ProductId = ProductId.NotEmpty(), Quantity = Quantity.NotNull().Positive() };
}

public record AddProductRequest(
    ProductItemRequest? ProductItem
);

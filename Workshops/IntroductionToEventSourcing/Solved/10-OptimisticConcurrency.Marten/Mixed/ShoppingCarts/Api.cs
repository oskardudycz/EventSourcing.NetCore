using Core.Validation;
using Marten;
using Marten.Schema.Identity;
using Microsoft.AspNetCore.Mvc;
using OptimisticConcurrency.Core.Http;
using OptimisticConcurrency.Core.Marten;
using OptimisticConcurrency.Mixed.Pricing;
using static Microsoft.AspNetCore.Http.TypedResults;
using static System.DateTimeOffset;

namespace OptimisticConcurrency.Mixed.ShoppingCarts;
using static ETagExtensions;

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

                var nextExpectedVersion = await session.Add<MixedShoppingCart>(shoppingCartId,
                    [MixedShoppingCart.Open(shoppingCartId, clientId.NotEmpty(), Now).Item1], ct);

                context.SetResponseEtag(nextExpectedVersion);

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

                var nextExpectedVersion = await session.GetAndUpdate<MixedShoppingCart>(shoppingCartId, ToExpectedVersion(eTag),
                    state => [state.AddProduct(pricingCalculator, productItem, Now)], ct);

                context.SetResponseEtag(nextExpectedVersion);

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

                var nextExpectedVersion = await session.GetAndUpdate<MixedShoppingCart>(shoppingCartId, ToExpectedVersion(eTag),
                    state => [state.RemoveProduct(productItem, Now)],
                    ct);

                context.SetResponseEtag(nextExpectedVersion);

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
                var nextExpectedVersion = await session.GetAndUpdate<MixedShoppingCart>(shoppingCartId, ToExpectedVersion(eTag),
                    state => [state.Confirm(Now)], ct);

                context.SetResponseEtag(nextExpectedVersion);

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
                var nextExpectedVersion = await session.GetAndUpdate<MixedShoppingCart>(shoppingCartId, ToExpectedVersion(eTag),
                    state => [state.Cancel(Now)], ct);

                context.SetResponseEtag(nextExpectedVersion);

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
                var result = await session.Events.AggregateStreamAsync<MixedShoppingCart>(shoppingCartId, token: ct);

                context.SetResponseEtag(result?.Version);

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

using ApplicationLogic.Marten.Core.Marten;
using ApplicationLogic.Marten.Mutable.Pricing;
using Core.Validation;
using Marten;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.TypedResults;
using static System.DateTimeOffset;

namespace ApplicationLogic.Marten.Mutable.ShoppingCarts;

public static class Api
{
    public static WebApplication ConfigureMutableShoppingCarts(this WebApplication app)
    {
        var clients = app.MapGroup("/api/mutable/clients/{clientId:guid}/");
        var shoppingCarts = clients.MapGroup("shopping-carts");
        var shoppingCart = shoppingCarts.MapGroup("{shoppingCartId:guid}");
        var productItems = shoppingCart.MapGroup("product-items");

        shoppingCarts.MapPost("",
            async (IDocumentSession session,
                Guid clientId,
                CancellationToken ct) =>
            {
                var shoppingCartId = Guid.CreateVersion7();

                await session.Add(shoppingCartId,
                    MutableShoppingCart.Open(shoppingCartId, clientId.NotEmpty(), Now), ct);

                return Created($"/api/mutable/clients/{clientId}/shopping-carts/{shoppingCartId}", shoppingCartId);
            }
        );

        productItems.MapPost("",
            async (
                IProductPriceCalculator pricingCalculator,
                IDocumentSession session,
                Guid shoppingCartId,
                AddProductRequest body,
                CancellationToken ct) =>
            {
                var productItem = body.ProductItem.NotNull().ToProductItem();

                await session.GetAndUpdate<MutableShoppingCart>(shoppingCartId,
                    state => state.AddProduct(pricingCalculator, productItem, Now), ct);

                return NoContent();
            }
        );

        productItems.MapDelete("{productId:guid}",
            async (
                IDocumentSession session,
                Guid shoppingCartId,
                [FromRoute] Guid productId,
                [FromQuery] int? quantity,
                [FromQuery] decimal? unitPrice,
                CancellationToken ct) =>
            {
                var productItem = new PricedProductItem
                {
                    ProductId = productId.NotEmpty(),
                    Quantity = quantity.NotNull().Positive(),
                    UnitPrice = unitPrice.NotNull().Positive()
                };

                await session.GetAndUpdate<MutableShoppingCart>(shoppingCartId,
                    state => state.RemoveProduct(productItem, Now),
                    ct);

                return NoContent();
            }
        );

        shoppingCart.MapPost("confirm",
            async (IDocumentSession session,
                Guid shoppingCartId,
                CancellationToken ct) =>
            {
                await session.GetAndUpdate<MutableShoppingCart>(shoppingCartId,
                    state => state.Confirm(Now), ct);

                return NoContent();
            }
        );

        shoppingCart.MapDelete("",
            async (IDocumentSession session,
                Guid shoppingCartId,
                CancellationToken ct) =>
            {
                await session.GetAndUpdate<MutableShoppingCart>(shoppingCartId,
                    state => state.Cancel(Now), ct);

                return NoContent();
            }
        );

        shoppingCart.MapGet("",
            async Task<IResult> (
                IQuerySession session,
                Guid shoppingCartId,
                CancellationToken ct) =>
            {
                var result = await session.Events.AggregateStreamAsync<MutableShoppingCart>(shoppingCartId, token: ct);

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
    public ProductItem ToProductItem() => new() { ProductId = ProductId.NotEmpty(), Quantity = Quantity.NotNull().Positive()};
}

public record AddProductRequest(
    ProductItemRequest? ProductItem
);

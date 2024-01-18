using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.Confirming;

using static ShoppingCartsApi;

public class ConfirmShoppingCartTests: IClassFixture<ApiSpecification<Program>>
{
    private readonly ApiSpecification<Program> API;

    public ConfirmShoppingCartTests(ApiSpecification<Program> api) => API = api;

    [Fact]
    [Trait("Category", "Acceptance")]
    public Task Put_Should_Return_OK_And_Confirm_Shopping_Cart() =>
        API
            .Given(
                "Shopping Cart with Product Item",
                OpenShoppingCart(ClientId),
                AddProductItem(ProductItem, ifMatch: 1)
            )
            .When(
                PUT,
                URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}/confirmation"),
                HEADERS(IF_MATCH(2))
            )
            .Then(OK)
            .And()
            .When(GET, URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}"))
            .Until(RESPONSE_ETAG_IS(3))
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>((details, ctx) =>
                {
                    details.Id.Should().Be(ctx.OpenedShoppingCartId());
                    details.Status.Should().Be(ShoppingCartStatus.Confirmed);
                    details.ProductItems.Count.Should().Be(1);
                    details.ProductItems.Single().ProductItem.Should()
                        .Be(Carts.ShoppingCarts.Products.ProductItem.From(ProductItem.ProductId, ProductItem.Quantity));
                    details.ClientId.Should().Be(ClientId);
                    details.Version.Should().Be(3);
                }));

    // API.PublishedExternalEventsOfType<CartFinalized>();

    private readonly Guid ClientId = Guid.NewGuid();

    private readonly ProductItemRequest ProductItem = new(Guid.NewGuid(), 1);
}

using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.Confirming;

using static ShoppingCartsApi;

public class ConfirmShoppingCartTests(ApiFixture fixture): ApiTest(fixture)
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public Task Put_Should_Return_OK_And_Confirm_Shopping_Cart() =>
        API
            .Given(
                "Shopping Cart with Product Item",
                OpenShoppingCart(clientId),
                AddProductItem(productItem, ifMatch: 1)
            )
            .When(
                PUT,
                URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}/confirmation"),
                HEADERS(IF_MATCH(2))
            )
            .Then(OK)
            .And()
            .When(GET, URI(ctx => $"/api/ShoppingCarts/{ctx.OpenedShoppingCartId()}"), HEADERS(IF_MATCH(3)))
            .Until(RESPONSE_ETAG_IS(3))
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>((details, ctx) =>
                {
                    details.Id.Should().Be(ctx.OpenedShoppingCartId());
                    details.Status.Should().Be(ShoppingCartStatus.Confirmed);
                    details.ProductItems.Count.Should().Be(1);
                    details.ProductItems.Single().ProductItem.Should()
                        .Be(Carts.ShoppingCarts.Products.ProductItem.From(productItem.ProductId, productItem.Quantity));
                    details.ClientId.Should().Be(clientId);
                    details.Version.Should().Be(3);
                }));

    // API.PublishedExternalEventsOfType<CartFinalized>();

    private readonly Guid clientId = Guid.CreateVersion7();

    private readonly ProductItemRequest productItem = new(Guid.CreateVersion7(), 1);
}

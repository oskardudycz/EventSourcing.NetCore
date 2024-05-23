using System.Globalization;
using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.Products;
using FluentAssertions;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.RemovingProduct;

public class RemoveProductTests(ApiFixture fixture): ApiTest(fixture), IAsyncLifetime
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public Task Delete_Should_Return_OK_And_Cancel_Shopping_Cart() =>
        API
            .Given()
            .When(
                DELETE,
                URI(
                    $"/api/ShoppingCarts/{ShoppingCartId}/products/{productItem.ProductId}?quantity={RemovedCount}&unitPrice={unitPrice.ToString(CultureInfo.InvariantCulture)}"),
                HEADERS(IF_MATCH(2))
            )
            .Then(NO_CONTENT)
            .And()
            .When(GET, URI($"/api/ShoppingCarts/{ShoppingCartId}"), HEADERS(IF_MATCH(3)))
            .Until(RESPONSE_ETAG_IS(3))
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>(details =>
                {
                    details.Id.Should().Be(ShoppingCartId);
                    details.Status.Should().Be(ShoppingCartStatus.Pending);
                    details.ProductItems.Should().HaveCount(1);
                    var productItem = details.ProductItems.Single();
                    productItem.Should().BeEquivalentTo(
                        PricedProductItem.Create(
                            ProductItem.From
                            (
                                this.productItem.ProductId!.Value,
                                this.productItem.Quantity!.Value - RemovedCount
                            ),
                            unitPrice
                        ));
                    details.ClientId.Should().Be(clientId);
                    details.Version.Should().Be(3);
                }));


    private Guid ShoppingCartId { get; set; }
    private readonly Guid clientId = Guid.NewGuid();
    private readonly ProductItemRequest productItem = new(Guid.NewGuid(), 10);
    private decimal unitPrice;
    private const int RemovedCount = 5;

    public async Task InitializeAsync()
    {
        var cartDetails = await API
            .Given()
            .When(POST, URI("/api/ShoppingCarts"), BODY(new OpenShoppingCartRequest(clientId)))
            .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 1))
            .And()
            .When(
                POST,
                URI(ctx => $"/api/ShoppingCarts/{ctx.GetCreatedId()}/products"),
                BODY(new AddProductRequest(productItem)),
                HEADERS(IF_MATCH(1))
            )
            .Then(OK)
            .And()
            .When(GET, URI(ctx => $"/api/ShoppingCarts/{ctx.GetCreatedId()}"), HEADERS(IF_MATCH(2)))
            .Until(RESPONSE_ETAG_IS(2))
            .Then(OK)
            .GetResponseBody<ShoppingCartDetails>();

        ShoppingCartId = cartDetails.Id;
        unitPrice = cartDetails.ProductItems.Single().UnitPrice;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

using System.Globalization;
using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace ECommerce.Api.Tests.ShoppingCarts.RemovingProduct;

public class RemoveProductTests(ApiFixture fixture): ApiTest(fixture), IAsyncLifetime
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Delete_Should_Return_OK_And_Cancel_Shopping_Cart() =>
        await API
            .Given()
            .When(
                DELETE,
                URI(
                    $"/api/ShoppingCarts/{ShoppingCartId}/products/{ProductItem.ProductId}?quantity={RemovedCount}&unitPrice={UnitPrice.ToString(CultureInfo.InvariantCulture)}"),
                HEADERS(IF_MATCH(1))
            )
            .Then(NO_CONTENT)
            .And()
            .When(GET, URI($"/api/ShoppingCarts/{ShoppingCartId}"), HEADERS(IF_MATCH(2)))
            .Until(RESPONSE_ETAG_IS(2), maxNumberOfRetries: 10)
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>(details =>
                {
                    details.Id.Should().Be(ShoppingCartId);
                    details.Status.Should().Be(ShoppingCartStatus.Pending);
                    details.ProductItems.Should().HaveCount(1);
                    var productItem = details.ProductItems.Single();
                    productItem.Should().BeEquivalentTo(
                        new ShoppingCartDetailsProductItem
                        {
                            ProductId = ProductItem.ProductId!.Value,
                            Quantity = ProductItem.Quantity!.Value - RemovedCount,
                            UnitPrice = UnitPrice
                        });
                    details.ClientId.Should().Be(ClientId);
                    details.Version.Should().Be(2);
                }));

    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public readonly ProductItemRequest ProductItem = new(Guid.NewGuid(), 10);

    public decimal UnitPrice;

    private readonly int RemovedCount = 5;

    public async Task InitializeAsync()
    {
        var cartDetails = await API.Given()
            .When(POST, URI("/api/ShoppingCarts"), BODY(new OpenShoppingCartRequest(ClientId)))
            .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 0))
            .And()
            .When(
                POST,
                URI(ctx => $"/api/ShoppingCarts/{ctx.GetCreatedId()}/products"),
                BODY(new AddProductRequest(ProductItem)),
                HEADERS(IF_MATCH(0))
            )
            .Then(OK)
            .And()
            .When(GET, URI(ctx => $"/api/ShoppingCarts/{ctx.GetCreatedId()}"), HEADERS(IF_MATCH(1)))
            .Until(RESPONSE_ETAG_IS(1), maxNumberOfRetries: 10)
            .Then(OK)
            .GetResponseBody<ShoppingCartDetails>();

        ShoppingCartId = cartDetails.Id;
        UnitPrice = cartDetails.ProductItems.Single().UnitPrice;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

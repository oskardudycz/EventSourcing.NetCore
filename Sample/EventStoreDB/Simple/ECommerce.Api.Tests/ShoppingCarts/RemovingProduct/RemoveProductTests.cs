using System.Globalization;
using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace ECommerce.Api.Tests.ShoppingCarts.RemovingProduct;

public class RemoveProductFixture(): ApiSpecification<Program>(new ShoppingCartsApplicationFactory()), IAsyncLifetime
{
    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public readonly ProductItemRequest ProductItem = new(Guid.NewGuid(), 10);

    public decimal UnitPrice;

    public async Task InitializeAsync()
    {
        var cartDetails = await Given()
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
            .When(GET, URI(ctx => $"/api/ShoppingCarts/{ctx.GetCreatedId()}"))
            .Until(RESPONSE_ETAG_IS(1), maxNumberOfRetries: 10)
            .Then(OK)
            .GetResponseBody<ShoppingCartDetails>();

        ShoppingCartId = cartDetails.Id;
        UnitPrice = cartDetails.ProductItems.Single().UnitPrice;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class RemoveProductTests(RemoveProductFixture api): IClassFixture<RemoveProductFixture>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Delete_Should_Return_OK_And_Cancel_Shopping_Cart()
    {
        await api
            .Given()
            .When(
                DELETE,
                URI(
                    $"/api/ShoppingCarts/{api.ShoppingCartId}/products/{api.ProductItem.ProductId}?quantity={RemovedCount}&unitPrice={api.UnitPrice.ToString(CultureInfo.InvariantCulture)}"),
                HEADERS(IF_MATCH(1))
            )
            .Then(NO_CONTENT)
            .And()
            .When(GET, URI($"/api/ShoppingCarts/{api.ShoppingCartId}"))
            .Until(RESPONSE_ETAG_IS(2), maxNumberOfRetries: 10)
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>(details =>
                {
                    details.Id.Should().Be(api.ShoppingCartId);
                    details.Status.Should().Be(ShoppingCartStatus.Pending);
                    details.ProductItems.Should().HaveCount(1);
                    var productItem = details.ProductItems.Single();
                    productItem.Should().BeEquivalentTo(
                        new ShoppingCartDetailsProductItem
                        {
                            ProductId = api.ProductItem.ProductId!.Value,
                            Quantity = api.ProductItem.Quantity!.Value - RemovedCount,
                            UnitPrice = api.UnitPrice
                        });
                    details.ClientId.Should().Be(api.ClientId);
                    details.Version.Should().Be(2);
                }));
    }

    private readonly int RemovedCount = 5;
}

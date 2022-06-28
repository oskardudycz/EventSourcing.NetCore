using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Xunit;
using Ogooreck.API;
using Warehouse.Api.Tests;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.RemovingProduct;

public class RemoveProductFixture: ApiSpecification<Program>, IAsyncLifetime
{
    public RemoveProductFixture(): base(new ShoppingCartsApplicationFactory()) { }
    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public readonly ProductItemRequest ProductItem = new(Guid.NewGuid(), 10);

    public decimal UnitPrice;

    public async Task InitializeAsync()
    {
        var openResponse = await Send(
            new ApiRequest(POST, URI("/api/ShoppingCarts"), BODY(new OpenShoppingCartRequest(ClientId)))
        );

        await CREATED_WITH_DEFAULT_HEADERS(eTag: 0)(openResponse);

        ShoppingCartId = openResponse.GetCreatedId<Guid>();

        var addResponse = await Send(
            new ApiRequest(
                POST,
                URI($"/api/ShoppingCarts/{ShoppingCartId}/products"),
                BODY(new AddProductRequest(ProductItem)),
                HEADERS(IF_MATCH(0)))
        );

        await OK(addResponse);

        var getResponse = await Send(
            new ApiRequest(
                GET_UNTIL(RESPONSE_ETAG_IS(1)),
                URI($"/api/ShoppingCarts/{ShoppingCartId}")
            )
        );

        var cartDetails = await getResponse.GetResultFromJson<ShoppingCartDetails>();
        UnitPrice = cartDetails.ProductItems.Single().UnitPrice;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class RemoveProductTests: IClassFixture<RemoveProductFixture>
{
    private readonly RemoveProductFixture API;

    public RemoveProductTests(RemoveProductFixture api) => API = api;

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Delete_Should_Return_OK_And_Cancel_Shopping_Cart()
    {
        await API
            .Given(
                URI(
                    $"/api/ShoppingCarts/{API.ShoppingCartId}/products/{API.ProductItem.ProductId}?quantity={RemovedCount}&unitPrice={API.UnitPrice}"),
                HEADERS(IF_MATCH(1))
            )
            .When(DELETE)
            .Then(NO_CONTENT);

        await API
            .Given(
                URI($"/api/ShoppingCarts/{API.ShoppingCartId}")
            )
            .When(GET_UNTIL(RESPONSE_ETAG_IS(2)))
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>(details =>
                {
                    details.Id.Should().Be(API.ShoppingCartId);
                    details.Status.Should().Be(ShoppingCartStatus.Pending);
                    details.ProductItems.Should().HaveCount(1);
                    var productItem = details.ProductItems.Single();
                    productItem.Should().BeEquivalentTo(
                        new ShoppingCartDetailsProductItem
                        {
                            ProductId = API.ProductItem.ProductId!.Value,
                            Quantity = API.ProductItem.Quantity!.Value - RemovedCount,
                            UnitPrice = API.UnitPrice
                        });
                    details.ClientId.Should().Be(API.ClientId);
                    details.Version.Should().Be(2);
                }));
    }

    private readonly int RemovedCount = 5;
}

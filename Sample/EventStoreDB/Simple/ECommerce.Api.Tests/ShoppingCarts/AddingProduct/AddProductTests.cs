using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Ogooreck.API;
using Warehouse.Api.Tests;
using static Ogooreck.API.ApiSpecification;
using Xunit;

namespace Carts.Api.Tests.ShoppingCarts.AddingProduct;

public class AddProductFixture: ApiSpecification<Program>, IAsyncLifetime
{
    public AddProductFixture(): base(new ShoppingCartsApplicationFactory()) { }

    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        var openResponse = await Send(
            new ApiRequest(POST, URI("/api/ShoppingCarts"), BODY(new OpenShoppingCartRequest(ClientId)))
        );

        await CREATED_WITH_DEFAULT_HEADERS(eTag: 0)(openResponse);

        ShoppingCartId = openResponse.GetCreatedId<Guid>();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class AddProductTests: IClassFixture<AddProductFixture>
{
    private readonly AddProductFixture API;

    public AddProductTests(AddProductFixture api) => API = api;

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Post_Should_AddProductItem_To_ShoppingCart()
    {
        var product = new ProductItemRequest(Guid.NewGuid(), 1);

        await API
            .Given(
                URI($"/api/ShoppingCarts/{API.ShoppingCartId}/products"),
                BODY(new AddProductRequest(product)),
                HEADERS(IF_MATCH(0))
            )
            .When(POST)
            .Then(OK);

        await API
            .Given(URI($"/api/ShoppingCarts/{API.ShoppingCartId}"))
            .When(GET_UNTIL(RESPONSE_ETAG_IS(1)))
            .Then(
                RESPONSE_BODY<ShoppingCartDetails>(details =>
                {
                    details.Id.Should().Be(API.ShoppingCartId);
                    details.Status.Should().Be(ShoppingCartStatus.Pending);
                    var productItem = details.ProductItems.Single();
                    productItem.Quantity.Should().Be(product.Quantity);
                    productItem.ProductId.Should().Be(product.ProductId!.Value);
                    details.Version.Should().Be(1);
                })
            );
    }
}

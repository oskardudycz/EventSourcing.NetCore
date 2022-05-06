using Carts.Api.Requests.Carts;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.Products;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;
using Xunit;

namespace Carts.Api.Tests.ShoppingCarts.AddingProduct;

public class AddProductFixture: ApiSpecification<Program>, IAsyncLifetime
{
    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        var openResponse = await Send(
            new ApiRequest(POST, URI("/api/ShoppingCarts"), BODY(new OpenShoppingCartRequest(ClientId)))
        );

        await CREATED(openResponse);

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
                HEADERS(IF_MATCH(1.ToString()))
            )
            .When(POST)
            .Then(OK);

        await API
            .Given(URI($"/api/ShoppingCarts/{API.ShoppingCartId}"))
            .When(GET)
            .Then(
                RESPONSE_BODY(
                    new ShoppingCartDetails
                    {
                        Id = API.ShoppingCartId,
                        Status = ShoppingCartStatus.Pending,
                        ClientId = API.ClientId,
                        ProductItems = new List<PricedProductItem>
                        {
                            PricedProductItem.Create(
                                ProductItem.Create(product.ProductId, product.Quantity),
                                100
                            )
                        },
                        Version = 2
                    }
                ));
    }
}

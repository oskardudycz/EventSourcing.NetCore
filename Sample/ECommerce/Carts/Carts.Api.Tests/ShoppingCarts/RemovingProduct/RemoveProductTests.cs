using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.Products;
using Xunit;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.RemovingProduct;

public class RemoveProductFixture: ApiSpecification<Program>, IAsyncLifetime
{
    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public readonly ProductItemRequest ProductItem = new(Guid.NewGuid(), 10);

    public decimal UnitPrice;

    public async Task InitializeAsync()
    {
        var openResponse = await Send(
            new ApiRequest(POST, URI("/api/ShoppingCarts"), BODY(new OpenShoppingCartRequest(ClientId)))
        );

        await CREATED_WITH_DEFAULT_HEADERS(eTag: 1)(openResponse);

        ShoppingCartId = openResponse.GetCreatedId<Guid>();

        var addResponse = await Send(
            new ApiRequest(
                POST,
                URI($"/api/ShoppingCarts/{ShoppingCartId}/products"),
                BODY(new AddProductRequest(ProductItem)),
                HEADERS(IF_MATCH(1)))
        );

        await OK(addResponse);

        var getResponse = await Send(
            new ApiRequest(
                GET_UNTIL(RESPONSE_ETAG_IS(2)),
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
                HEADERS(IF_MATCH(2))
            )
            .When(DELETE)
            .Then(NO_CONTENT);

        await API
            .Given(
                URI($"/api/ShoppingCarts/{API.ShoppingCartId}")
            )
            .When(GET_UNTIL(RESPONSE_ETAG_IS(3)))
            .Then(
                OK,
                RESPONSE_BODY(new ShoppingCartDetails
                {
                    Id = API.ShoppingCartId,
                    Status = ShoppingCartStatus.Pending,
                    ClientId = API.ClientId,
                    ProductItems = new List<PricedProductItem>
                    {
                        PricedProductItem.Create(
                            ProductItem.From(API.ProductItem.ProductId, API.ProductItem.Quantity - RemovedCount),
                            API.UnitPrice
                        )
                    },
                    Version = 3,
                }));
    }

    private readonly int RemovedCount = 5;
}

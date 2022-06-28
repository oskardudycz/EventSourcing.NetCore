using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.Products;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.Canceling;

public class CancelShoppingCartFixture: ApiSpecification<Program>, IAsyncLifetime
{
    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        var openResponse = await Send(
            new ApiRequest(POST, URI("/api/ShoppingCarts"), BODY(new OpenShoppingCartRequest(ClientId)))
        );

        await CREATED_WITH_DEFAULT_HEADERS(eTag: 1)(openResponse);

        ShoppingCartId = openResponse.GetCreatedId<Guid>();
    }

    public Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }
}

public class CancelShoppingCartTests: IClassFixture<CancelShoppingCartFixture>
{
    private readonly CancelShoppingCartFixture API;

    public CancelShoppingCartTests(CancelShoppingCartFixture api) => API = api;

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Delete_Should_Return_OK_And_Cancel_Shopping_Cart()
    {
        await API
            .Given(
                URI($"/api/ShoppingCarts/{API.ShoppingCartId}"),
                HEADERS(IF_MATCH(1))
            )
            .When(DELETE)
            .Then(OK);

        await API
            .Given(
                URI($"/api/ShoppingCarts/{API.ShoppingCartId}")
            )
            .When(GET_UNTIL(RESPONSE_ETAG_IS(2)))
            .Then(
                OK,
                RESPONSE_BODY(new ShoppingCartDetails
                {
                    Id = API.ShoppingCartId,
                    Status = ShoppingCartStatus.Canceled,
                    ProductItems = new List<PricedProductItem>(),
                    ClientId = API.ClientId,
                    Version = 2,
                }));

        // API.PublishedExternalEventsOfType<CartFinalized>();
    }
}

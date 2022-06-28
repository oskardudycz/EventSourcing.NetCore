using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.Products;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Carts.Api.Tests.ShoppingCarts.Confirming;

public class ConfirmShoppingCartFixture: ApiSpecification<Program>, IAsyncLifetime
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

    public Task DisposeAsync() => Task.CompletedTask;
}

public class ConfirmShoppingCartTests: IClassFixture<ConfirmShoppingCartFixture>
{

    private readonly ConfirmShoppingCartFixture API;

    public ConfirmShoppingCartTests(ConfirmShoppingCartFixture api) => API = api;

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Put_Should_Return_OK_And_Cancel_Shopping_Cart()
    {
        await API
            .Given(
                URI($"/api/ShoppingCarts/{API.ShoppingCartId}/confirmation"),
                HEADERS(IF_MATCH(1))
            )
            .When(PUT)
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
                    Status = ShoppingCartStatus.Confirmed,
                    ProductItems = new List<PricedProductItem>(),
                    ClientId = API.ClientId,
                    Version = 2,
                }));

        // API.PublishedExternalEventsOfType<CartFinalized>();
    }
}

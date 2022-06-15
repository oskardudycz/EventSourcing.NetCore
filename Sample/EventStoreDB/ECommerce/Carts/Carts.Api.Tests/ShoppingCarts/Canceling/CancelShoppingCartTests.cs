using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.Products;
using FluentAssertions;
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

        await CREATED(openResponse);

        ShoppingCartId = openResponse.GetCreatedId<Guid>();
    }

    public Task DisposeAsync() => Task.CompletedTask;
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
                HEADERS(IF_MATCH(0))
            )
            .When(DELETE)
            .Then(OK);

        await API
            .Given(
                URI($"/api/ShoppingCarts/{API.ShoppingCartId}")
            )
            .When(GET_UNTIL(RESPONSE_ETAG_IS(1)))
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>(details =>
                {
                    details.Id.Should().Be(API.ShoppingCartId);
                    details.Status.Should().Be(ShoppingCartStatus.Canceled);
                    details.ProductItems.Should().BeEmpty();
                    details.ClientId.Should().Be(API.ClientId);
                    details.Version.Should().Be(1);
                }));

        // API.PublishedExternalEventsOfType<CartFinalized>();
    }
}

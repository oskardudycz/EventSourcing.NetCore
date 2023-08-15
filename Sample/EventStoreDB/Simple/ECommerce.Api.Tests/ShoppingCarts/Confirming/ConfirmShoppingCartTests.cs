using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace ECommerce.Api.Tests.ShoppingCarts.Confirming;

public class ConfirmShoppingCartFixture: ApiSpecification<Program>, IAsyncLifetime
{
    public ConfirmShoppingCartFixture(): base(new ShoppingCartsApplicationFactory()) { }

    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        ShoppingCartId = await Given()
            .When(POST, URI("/api/ShoppingCarts"), BODY(new OpenShoppingCartRequest(ClientId)))
            .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 0))
            .GetCreatedId<Guid>();
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
            .Given()
            .When(
                PUT,
                URI($"/api/ShoppingCarts/{API.ShoppingCartId}/confirmation"),
                HEADERS(IF_MATCH(0))
            )
            .Then(OK);

        await API
            .Given()
            .When(GET, URI($"/api/ShoppingCarts/{API.ShoppingCartId}"))
            .Until(RESPONSE_ETAG_IS(1))
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>(details =>
                {
                    details.Id.Should().Be(API.ShoppingCartId);
                    details.Status.Should().Be(ShoppingCartStatus.Confirmed);
                    details.ProductItems.Should().BeEmpty();
                    details.ClientId.Should().Be(API.ClientId);
                    details.Version.Should().Be(1);
                }));

        // API.PublishedExternalEventsOfType<CartFinalized>();
    }
}

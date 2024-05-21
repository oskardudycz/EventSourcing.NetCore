using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using FluentAssertions;
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
        ShoppingCartId = await Given()
            .When(POST, URI("/api/ShoppingCarts"), BODY(new OpenShoppingCartRequest(ClientId)))
            .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 0))
            .GetCreatedId<Guid>();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class ConfirmShoppingCartTests(ConfirmShoppingCartFixture api): IClassFixture<ConfirmShoppingCartFixture>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Put_Should_Return_OK_And_Cancel_Shopping_Cart()
    {
        await api
            .Given()
            .When(
                PUT,
                URI($"/api/ShoppingCarts/{api.ShoppingCartId}/confirmation"),
                HEADERS(IF_MATCH(0))
            )
            .Then(OK);

        await api
            .Given()
            .When(GET, URI($"/api/ShoppingCarts/{api.ShoppingCartId}"))
            .Until(RESPONSE_ETAG_IS(1), 10)
            .Then(
                OK,
                RESPONSE_BODY<ShoppingCartDetails>(details =>
                {
                    details.Id.Should().Be(api.ShoppingCartId);
                    details.Status.Should().Be(ShoppingCartStatus.Confirmed);
                    details.ProductItems.Should().BeEmpty();
                    details.ClientId.Should().Be(api.ClientId);
                    details.Version.Should().Be(1);
                }));

        // API.PublishedExternalEventsOfType<CartFinalized>();
    }
}

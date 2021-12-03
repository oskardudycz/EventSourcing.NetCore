using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Carts.Api.Requests.Carts;
using Carts.Carts;
using Carts.Carts.FinalizingCart;
using Carts.Carts.GettingCartById;
using Core.Api.Testing;
using Core.Testing;
using FluentAssertions;
using Xunit;

namespace Carts.Api.Tests.Carts.AddingProduct;

public class AddProductFixture: ApiWithEventsFixture<Startup>
{
    protected override string ApiUrl => "/api/Carts";

    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public readonly ProductItemRequest ProductItem = new(Guid.NewGuid(), 1);

    public HttpResponseMessage CommandResponse = default!;

    public override async Task InitializeAsync()
    {
        var initializeResponse = await Post(new InitCartRequest(ClientId));
        initializeResponse.EnsureSuccessStatusCode();

        ShoppingCartId = await initializeResponse.GetResultFromJson<Guid>();

        CommandResponse = await Post($"{ShoppingCartId}/products", new AddProductRequest(ProductItem));
    }
}

public class AddProductTests: IClassFixture<AddProductFixture>
{
    private readonly AddProductFixture fixture;

    public AddProductTests(AddProductFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public Task Put_Should_Return_OK()
    {
        var commandResponse = fixture.CommandResponse.EnsureSuccessStatusCode();
        commandResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        return Task.CompletedTask;
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Put_Should_Confirm_ShoppingCart()
    {
        // prepare query
        var query = $"{fixture.ShoppingCartId}";

        //send query
        var queryResponse = await fixture.Get(query, 30,
            check: async response => (await response.GetResultFromJson<CartDetails>()).Version == 2);

        queryResponse.EnsureSuccessStatusCode();

        var cartDetails = await queryResponse.GetResultFromJson<CartDetails>();
        cartDetails.Should().NotBeNull();
        cartDetails.Id.Should().Be(fixture.ShoppingCartId);
        cartDetails.Status.Should().Be(CartStatus.Pending);
        cartDetails.ClientId.Should().Be(fixture.ClientId);
        cartDetails.ProductItems.Should().HaveCount(1);

        var productItem = cartDetails.ProductItems.Single().ProductItem;
        productItem.ProductId.Should().Be(fixture.ProductItem.ProductId);
        productItem.Quantity.Should().Be(fixture.ProductItem.Quantity);
        cartDetails.Version.Should().Be(2);

        fixture.PublishedExternalEventsOfType<CartFinalized>();
    }
}

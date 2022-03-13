using System.Net;
using Carts.Api.Requests.Carts;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Core.Api.Testing;
using Core.Testing;
using FluentAssertions;
using Xunit;

namespace Carts.Api.Tests.ShoppingCarts.AddingProduct;

public class AddProductFixture: ApiWithEventsFixture<Startup>
{
    protected override string ApiUrl => "/api/ShoppingCarts";

    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public readonly ProductItemRequest ProductItem = new(Guid.NewGuid(), 1);

    public HttpResponseMessage CommandResponse = default!;

    public override async Task InitializeAsync()
    {
        var openResponse = await Post(new OpenShoppingCartRequest(ClientId));
        openResponse.EnsureSuccessStatusCode();

        ShoppingCartId = await openResponse.GetResultFromJson<Guid>();

        CommandResponse = await Post(
            $"{ShoppingCartId}/products",
            new AddProductRequest(ProductItem),
            new RequestOptions { IfMatch = 1.ToString() }
        );
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
    public void Put_Should_Return_OK()
    {
        var commandResponse = fixture.CommandResponse.EnsureSuccessStatusCode();
        commandResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Put_Should_AddProductTo_ShoppingCart()
    {
        // prepare query
        var query = $"{fixture.ShoppingCartId}";

        //send query
        var queryResponse = await fixture.Get(query, 30,
            check: async response => (await response.GetResultFromJson<ShoppingCartDetails>()).Version == 2);

        queryResponse.EnsureSuccessStatusCode();

        var cartDetails = await queryResponse.GetResultFromJson<ShoppingCartDetails>();
        cartDetails.Should().NotBeNull();
        cartDetails.Version.Should().Be(2);
        cartDetails.Id.Should().Be(fixture.ShoppingCartId);
        cartDetails.Status.Should().Be(ShoppingCartStatus.Pending);
        cartDetails.ClientId.Should().Be(fixture.ClientId);
        cartDetails.ProductItems.Should().HaveCount(1);

        var productItem = cartDetails.ProductItems.Single();
        productItem.ProductId.Should().Be(fixture.ProductItem.ProductId!.Value);
        productItem.Quantity.Should().Be(fixture.ProductItem.Quantity);
    }
}

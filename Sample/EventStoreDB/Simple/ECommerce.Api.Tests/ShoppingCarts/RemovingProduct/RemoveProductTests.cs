using System.Net;
using Core.Api.Testing;
using ECommerce.Api.Requests;
using ECommerce.ShoppingCarts;
using ECommerce.ShoppingCarts.GettingCartById;
using FluentAssertions;
using Xunit;

namespace ECommerce.Api.Tests.ShoppingCarts.RemovingProduct;

public class RemoveProductFixture: ApiFixture<Startup>
{
    protected override string ApiUrl => "/api/ShoppingCarts";

    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public readonly ProductItemRequest ProductItem = new(Guid.NewGuid(), 10);

    public readonly int RemovedCount = 5;

    public HttpResponseMessage CommandResponse = default!;

    public override async Task InitializeAsync()
    {
        var openResponse = await Post(new OpenShoppingCartRequest(ClientId));
        openResponse.EnsureSuccessStatusCode();

        ShoppingCartId = await openResponse.GetResultFromJson<Guid>();

        var addResponse  = await Post(
            $"{ShoppingCartId}/products",
            new AddProductRequest(ProductItem),
            new RequestOptions { IfMatch = 0.ToString() }
        );
        addResponse.EnsureSuccessStatusCode();

        var queryResponse = await Get($"{ShoppingCartId}", 30,
            check: async response => (await response.GetResultFromJson<ShoppingCartDetails>()).Version == 1);
        queryResponse.EnsureSuccessStatusCode();

        var cartDetails = await queryResponse.GetResultFromJson<ShoppingCartDetails>();
        var unitPrice = cartDetails.ProductItems.Single().UnitPrice;

        CommandResponse = await Delete(
            $"{ShoppingCartId}/products/{ProductItem.ProductId}?quantity={RemovedCount}&unitPrice={unitPrice}",
            new RequestOptions { IfMatch = 1.ToString() }
        );
    }
}

public class RemoveProductTests: IClassFixture<RemoveProductFixture>
{
    private readonly RemoveProductFixture fixture;

    public RemoveProductTests(RemoveProductFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public void Delete_Should_Return_OK()
    {
        var commandResponse = fixture.CommandResponse.EnsureSuccessStatusCode();
        commandResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Delete_Should_RemoveProductFrom_ShoppingCart()
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
        productItem.Quantity.Should().Be(fixture.ProductItem.Quantity - fixture.RemovedCount);
    }
}

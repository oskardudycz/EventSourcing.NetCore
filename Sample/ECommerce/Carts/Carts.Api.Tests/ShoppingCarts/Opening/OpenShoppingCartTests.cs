using System.Net;
using Carts.Api.Requests.Carts;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Core.Api.Testing;
using Core.Testing;
using FluentAssertions;
using Xunit;

namespace Carts.Api.Tests.ShoppingCarts.Opening;

public class OpenShoppingCartFixture: ApiWithEventsFixture<Startup>
{
    protected override string ApiUrl => "/api/ShoppingCarts";

    public readonly Guid ClientId = Guid.NewGuid();

    public HttpResponseMessage CommandResponse = default!;

    public override async Task InitializeAsync()
    {
        CommandResponse = await Post(new OpenShoppingCartRequest(ClientId));
    }
}

public class OpenShoppingCartTests: IClassFixture<OpenShoppingCartFixture>
{
    private readonly OpenShoppingCartFixture fixture;

    public OpenShoppingCartTests(OpenShoppingCartFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Post_Should_Return_CreatedStatus_With_CartId()
    {
        var commandResponse = fixture.CommandResponse.EnsureSuccessStatusCode();
        commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // get created record id
        var createdId = await commandResponse.GetResultFromJson<Guid>();
        createdId.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Post_Should_Create_ShoppingCart()
    {
        var createdId = await fixture.CommandResponse.GetResultFromJson<Guid>();

        // prepare query
        var query = $"{createdId}";

        //send query
        var queryResponse = await fixture.Get(query, 30);

        queryResponse.EnsureSuccessStatusCode();

        var cartDetails = await queryResponse.GetResultFromJson<ShoppingCartDetails>();
        cartDetails.Should().NotBeNull();
        cartDetails.Id.Should().Be(createdId);
        cartDetails.Status.Should().Be(ShoppingCartStatus.Pending);
        cartDetails.ClientId.Should().Be(fixture.ClientId);
        cartDetails.Version.Should().Be(1);
    }
}

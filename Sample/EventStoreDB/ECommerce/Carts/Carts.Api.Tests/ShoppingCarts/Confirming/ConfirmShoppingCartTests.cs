using System.Net;
using Carts.Api.Requests;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.GettingCartById;
using Core.Api.Testing;
using FluentAssertions;
using Xunit;

namespace Carts.Api.Tests.ShoppingCarts.Confirming;

public class ConfirmShoppingCartFixture: ApiFixture<Startup>
{
    protected override string ApiUrl => "/api/ShoppingCarts";

    public Guid ShoppingCartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public HttpResponseMessage CommandResponse = default!;

    public override async Task InitializeAsync()
    {
        var openResponse = await Post(new OpenShoppingCartRequest(ClientId));
        openResponse.EnsureSuccessStatusCode();

        ShoppingCartId = await openResponse.GetResultFromJson<Guid>();

        CommandResponse = await Put(
            $"{ShoppingCartId}/confirmation",
            new ConfirmShoppingCartRequest(),
            new RequestOptions { IfMatch = 0.ToString() }
        );
    }
}

public class ConfirmShoppingCartTests: IClassFixture<ConfirmShoppingCartFixture>
{
    private readonly ConfirmShoppingCartFixture fixture;

    public ConfirmShoppingCartTests(ConfirmShoppingCartFixture fixture)
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
    public async Task Put_Should_Confirm_ShoppingCart()
    {
        // prepare query
        var query = $"{fixture.ShoppingCartId}";

        //send query
        var queryResponse = await fixture.Get(query, 30,
            check: async response => (await response.GetResultFromJson<ShoppingCartDetails>()).Version == 1);

        queryResponse.EnsureSuccessStatusCode();

        var cartDetails = await queryResponse.GetResultFromJson<ShoppingCartDetails>();
        cartDetails.Should().NotBeNull();
        cartDetails.Id.Should().Be(fixture.ShoppingCartId);
        cartDetails.Status.Should().Be(ShoppingCartStatus.Confirmed);
        cartDetails.ClientId.Should().Be(fixture.ClientId);
        cartDetails.Version.Should().Be(1);
    }
}

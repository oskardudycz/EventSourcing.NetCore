using System.Net;
using Core.Api.Testing;
using Core.Testing;
using FluentAssertions;
using Orders.Api.Requests.Carts;
using Orders.Orders.InitializingOrder;
using Xunit;

namespace Orders.Api.Tests.Orders.InitializingOrder;

public class InitializeOrderFixture: ApiWithEventsFixture<Startup>
{
    protected override string ApiUrl => "/api/Orders";

    public readonly Guid ClientId = Guid.NewGuid();

    public readonly List<PricedProductItemRequest> ProductItems = new()
    {
        new PricedProductItemRequest { ProductId = Guid.NewGuid(), Quantity = 10, UnitPrice = 3 },
        new PricedProductItemRequest { ProductId = Guid.NewGuid(), Quantity = 3, UnitPrice = 7 }
    };

    public decimal TotalPrice => ProductItems.Sum(pi => pi.Quantity!.Value * pi.UnitPrice!.Value);

    public readonly DateTime TimeBeforeSending = DateTime.UtcNow;

    public HttpResponseMessage CommandResponse = default!;

    public override async Task InitializeAsync()
    {
        CommandResponse = await Post(new InitOrderRequest(
            ClientId,
            ProductItems,
            TotalPrice
        ));
    }
}

public class InitializeOrderTests: IClassFixture<InitializeOrderFixture>
{
    private readonly InitializeOrderFixture fixture;

    public InitializeOrderTests(InitializeOrderFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task InitializeOrder_ShouldReturn_CreatedStatus_With_OrderId()
    {
        var commandResponse = fixture.CommandResponse;
        commandResponse.EnsureSuccessStatusCode();
        commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // get created record id
        var createdId = await commandResponse.GetResultFromJson<Guid>();
        createdId.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task InitializeOrder_ShouldPublish_OrderInitializedEvent()
    {
        var createdId = await fixture.CommandResponse.GetResultFromJson<Guid>();

        await fixture.ShouldPublishInternalEventOfType<OrderInitialized>(
            @event =>
                @event.OrderId == createdId
                && @event.ClientId == fixture.ClientId
                && @event.InitializedAt > fixture.TimeBeforeSending
                && @event.ProductItems.Count == fixture.ProductItems.Count
                && @event.ProductItems.All(
                    pi => fixture.ProductItems.Exists(
                        expi => expi.ProductId == pi.ProductId && expi.Quantity == pi.Quantity)
                    )
        );
    }

    // [Fact]
    // [Trait("Category", "Acceptance")]
    // public async Task CreateCommand_ShouldCreate_Order()
    // {
    //     var createdId = await fixture.CommandResponse.GetResultFromJson<Guid>();
    //
    //     // prepare query
    //     var query = $"{createdId}";
    //
    //     //send query
    //     var queryResponse = await fixture.Get(query);
    //     queryResponse.EnsureSuccessStatusCode();
    //
    //     var queryResult = await queryResponse.Content.ReadAsStringAsync();
    //     queryResult.Should().NotBeNull();
    //
    //     var OrderDetails = queryResult.FromJson<Order>();
    //     OrderDetails.Id.Should().Be(createdId);
    //     OrderDetails.OrderId.Should().Be(fixture.ClientId);
    //     OrderDetails.SentAt.Should().BeAfter(fixture.TimeBeforeSending);
    //     OrderDetails.ProductItems.Should().NotBeEmpty();
    //     OrderDetails.ProductItems.All(
    //         pi => fixture.ProductItems.Exists(
    //             expi => expi.ProductId == pi.ProductId && expi.Quantity == pi.Quantity))
    //         .Should().BeTrue();
    // }
}

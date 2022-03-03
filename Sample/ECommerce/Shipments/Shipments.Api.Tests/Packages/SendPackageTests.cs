using System.Net;
using Core.Api.Testing;
using Core.Testing;
using FluentAssertions;
using Shipments.Packages;
using Shipments.Packages.Events.External;
using Shipments.Packages.Requests;
using Shipments.Products;
using Xunit;

namespace Shipments.Api.Tests.Packages;

public class SendPackageFixture: ApiWithEventsFixture<Startup>
{
    protected override string ApiUrl => "/api/Shipments";

    public readonly Guid OrderId = Guid.NewGuid();

    public readonly DateTime TimeBeforeSending = DateTime.UtcNow;

    public readonly List<ProductItem> ProductItems = new()
    {
        new ProductItem { ProductId = Guid.NewGuid(), Quantity = 10 },
        new ProductItem { ProductId = Guid.NewGuid(), Quantity = 3 }
    };

    public HttpResponseMessage CommandResponse = default!;

    public override async Task InitializeAsync()
    {
        CommandResponse = await Post(new SendPackage(OrderId, ProductItems));
    }
}

public class SendPackageTests: IClassFixture<SendPackageFixture>
{
    private readonly SendPackageFixture fixture;

    public SendPackageTests(SendPackageFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task SendPackage_ShouldReturn_CreatedStatus_With_PackageId()
    {
        var commandResponse = fixture.CommandResponse.EnsureSuccessStatusCode();
        commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // get created record id
        var createdId = await commandResponse.GetResultFromJson<Guid>();
        createdId.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task SendPackage_ShouldPublish_PackageWasSentEvent()
    {
        var createdId = await fixture.CommandResponse.GetResultFromJson<Guid>();

        await fixture.ShouldPublishInternalEventOfType<PackageWasSent>(
            @event =>
                @event.PackageId == createdId
                && @event.OrderId == fixture.OrderId
                && @event.SentAt > fixture.TimeBeforeSending
                && @event.ProductItems.Count == fixture.ProductItems.Count
                && @event.ProductItems.All(
                    pi => fixture.ProductItems.Exists(
                        expi => expi.ProductId == pi.ProductId && expi.Quantity == pi.Quantity))
        );
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task SendPackage_ShouldCreate_Package()
    {
        var createdId = await fixture.CommandResponse.GetResultFromJson<Guid>();

        // prepare query
        var query = $"{createdId}";

        //send query
        var queryResponse = await fixture.Get(query);
        queryResponse.EnsureSuccessStatusCode();

        var packageDetails = await queryResponse.GetResultFromJson<Package>();
        packageDetails.Id.Should().Be(createdId);
        packageDetails.OrderId.Should().Be(fixture.OrderId);
        packageDetails.SentAt.Should().BeAfter(fixture.TimeBeforeSending);
        packageDetails.ProductItems.Should().NotBeEmpty();
        packageDetails.ProductItems.All(
                pi => fixture.ProductItems.Exists(
                    expi => expi.ProductId == pi.ProductId && expi.Quantity == pi.Quantity))
            .Should().BeTrue();
    }
}

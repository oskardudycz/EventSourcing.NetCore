using Core.Testing;
using Ogooreck.API;
using Shipments.Packages.Events.External;
using Shipments.Packages.Requests;
using Shipments.Products;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Shipments.Api.Tests.Packages;

public class SendPackageTests: IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly ApiSpecification<Program> API;
    private readonly TestWebApplicationFactory<Program> fixture;

    public SendPackageTests(TestWebApplicationFactory<Program> fixture)
    {
        this.fixture = fixture;
        API = ApiSpecification<Program>.Setup(fixture);
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public Task SendPackage_ShouldReturn_CreatedStatus_With_PackageId() =>
        API.Given()
            .When(
                POST,
                URI("/api/Shipments/"),
                BODY(new SendPackage(OrderId, ProductItems))
            )
            .Then(CREATED_WITH_DEFAULT_HEADERS())
            .And(response => fixture.ShouldPublishInternalEventOfType<PackageWasSent>(
                @event =>
                    @event.PackageId == response.GetCreatedId<Guid>()
                    && @event.OrderId == OrderId
                    && @event.SentAt > TimeBeforeSending
                    && @event.ProductItems.Count == ProductItems.Count
                    && @event.ProductItems.All(
                        pi => ProductItems.Exists(
                            expi => expi.ProductId == pi.ProductId && expi.Quantity == pi.Quantity))
            ));

    private readonly Guid OrderId = Guid.NewGuid();

    private readonly DateTime TimeBeforeSending = DateTime.UtcNow;

    private readonly List<ProductItem> ProductItems = new()
    {
        new ProductItem { ProductId = Guid.NewGuid(), Quantity = 10 },
        new ProductItem { ProductId = Guid.NewGuid(), Quantity = 3 }
    };
}

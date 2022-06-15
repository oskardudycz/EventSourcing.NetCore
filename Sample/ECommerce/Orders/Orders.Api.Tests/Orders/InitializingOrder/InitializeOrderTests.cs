using Core.Testing;
using Orders.Api.Requests.Carts;
using Xunit;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace Orders.Api.Tests.Orders.InitializingOrder;

public class InitializeOrderTests: IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly ApiSpecification<Program> API;
    private readonly TestWebApplicationFactory<Program> fixture;

    public InitializeOrderTests(TestWebApplicationFactory<Program> fixture)
    {
        this.fixture = fixture;
        API = ApiSpecification<Program>.Setup(fixture);
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public Task InitializeOrder_ShouldReturn_CreatedStatus_With_OrderId() =>
        API.Given(
                URI("/api/Orders/"),
                BODY(new InitOrderRequest(
                    ClientId,
                    ProductItems,
                    TotalPrice
                ))
            )
            .When(POST)
            .Then(CREATED);

    private readonly Guid ClientId = Guid.NewGuid();

    private readonly List<PricedProductItemRequest> ProductItems = new()
    {
        new PricedProductItemRequest { ProductId = Guid.NewGuid(), Quantity = 10, UnitPrice = 3 },
        new PricedProductItemRequest { ProductId = Guid.NewGuid(), Quantity = 3, UnitPrice = 7 }
    };

    private decimal TotalPrice => ProductItems.Sum(pi => pi.Quantity!.Value * pi.UnitPrice!.Value);
}

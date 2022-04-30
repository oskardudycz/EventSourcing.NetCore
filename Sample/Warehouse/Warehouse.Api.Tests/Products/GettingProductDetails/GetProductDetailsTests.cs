using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;
using Warehouse.Products.GettingProductDetails;
using Warehouse.Products.RegisteringProduct;
using Xunit;

namespace Warehouse.Api.Tests.Products.GettingProductDetails;

public class GetProductDetailsTests: IClassFixture<GetProductDetailsFixture>
{
    private readonly GetProductDetailsFixture API;

    public GetProductDetailsTests(GetProductDetailsFixture api) => API = api;

    [Fact]
    public Task ValidRequest_With_NoParams_ShouldReturn_200() =>
        API.Given(URI($"/api/products/{API.ExistingProduct.Id}"))
            .When(GET)
            .Then(OK, RESPONSE_BODY(API.ExistingProduct));

    [Theory]
    [InlineData(12)]
    [InlineData("not-a-guid")]
    public Task InvalidGuidId_ShouldReturn_400(object invalidId) =>
        API.Given(URI($"/api/products/{invalidId}"))
            .When(GET)
            .Then(BAD_REQUEST);

    [Fact]
    public Task NotExistingId_ShouldReturn_404() =>
        API.Given(URI($"/api/products/{Guid.NewGuid()}"))
            .When(GET)
            .Then(NOT_FOUND);
}


public class GetProductDetailsFixture: ApiSpecification<Program>, IAsyncLifetime
{
    public ProductDetails ExistingProduct = default!;

    public GetProductDetailsFixture(): base(new WarehouseTestWebApplicationFactory()) { }

    public async Task InitializeAsync()
    {
        var registerProduct = new RegisterProductRequest("IN11111", "ValidName", "ValidDescription");
        var registerResponse = await Send(
            new ApiRequest(POST, URI("/api/products"), BODY(registerProduct))
        );

        await CREATED(registerResponse);

        var (sku, name, description) = registerProduct;
        ExistingProduct = new ProductDetails(registerResponse.GetCreatedId<Guid>(), sku!, name!, description);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

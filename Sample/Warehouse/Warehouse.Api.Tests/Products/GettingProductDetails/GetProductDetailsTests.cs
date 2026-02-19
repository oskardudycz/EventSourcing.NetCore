using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;
using Warehouse.Products.GettingProductDetails;
using Warehouse.Products.RegisteringProduct;
using Xunit;

namespace Warehouse.Api.Tests.Products.GettingProductDetails;

public class GetProductDetailsTests(GetProductDetailsFixture api): IClassFixture<GetProductDetailsFixture>
{
    [Fact]
    public Task ValidRequest_With_NoParams_ShouldReturn_200() =>
        api.Given()
            .When(GET, URI($"/api/products/{api.ExistingProduct.Id}"))
            .Then(OK, RESPONSE_BODY(api.ExistingProduct));

    [Theory]
    [InlineData(12)]
    [InlineData("not-a-guid")]
    public Task InvalidGuidId_ShouldReturn_404(object invalidId) =>
        api.Given()
            .When(GET, URI($"/api/products/{invalidId}"))
            .Then(NOT_FOUND);

    [Fact]
    public Task NotExistingId_ShouldReturn_404() =>
        api.Given()
            .When(GET, URI($"/api/products/{Guid.CreateVersion7()}"))
            .Then(NOT_FOUND);
}


public class GetProductDetailsFixture()
    : ApiSpecification<Program>(new WarehouseTestWebApplicationFactory()), IAsyncLifetime
{
    public ProductDetails ExistingProduct = null!;

    public async Task InitializeAsync()
    {
        var registerProduct = new RegisterProductRequest("IN11111", "ValidName", "ValidDescription");
        var productId = await Given()
            .When(POST, URI("/api/products"), BODY(registerProduct))
            .Then(CREATED)
            .GetCreatedId<Guid>();

        var (sku, name, description) = registerProduct;
        ExistingProduct = new ProductDetails(productId, sku!, name!, description);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

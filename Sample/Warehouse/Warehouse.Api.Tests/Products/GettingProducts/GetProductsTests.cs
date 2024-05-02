using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;
using Warehouse.Products.GettingProducts;
using Warehouse.Products.RegisteringProduct;
using Xunit;

namespace Warehouse.Api.Tests.Products.GettingProducts;

public class GetProductsTests(GetProductsFixture api): IClassFixture<GetProductsFixture>
{
    [Fact]
    public Task ValidRequest_With_NoParams_ShouldReturn_200() =>
        api.Given()
            .When(GET, URI("/api/products/"))
            .Then(OK, RESPONSE_BODY(api.RegisteredProducts));

    [Fact]
    public Task ValidRequest_With_Filter_ShouldReturn_SubsetOfRecords()
    {
        var registeredProduct = api.RegisteredProducts.First();
        var filter = registeredProduct.Sku[1..];

        return api.Given()
            .When(GET, URI($"/api/products/?filter={filter}"))
            .Then(OK, RESPONSE_BODY(new List<ProductListItem> { registeredProduct }));
    }

    [Fact]
    public Task ValidRequest_With_Paging_ShouldReturn_PageOfRecords()
    {
        // Given
        const int page = 2;
        const int pageSize = 1;
        var pagedRecords = api.RegisteredProducts
            .Skip(page - 1)
            .Take(pageSize)
            .ToList();

        return api.Given()
            .When(GET, URI($"/api/products/?page={page}&pageSize={pageSize}"))
            .Then(OK, RESPONSE_BODY(pagedRecords));
    }

    [Fact]
    public Task NegativePage_ShouldReturn_400() =>
        api.Given()
            .When(GET, URI($"/api/products/?page={-20}"))
            .Then(BAD_REQUEST);

    [Theory]
    [InlineData(0)]
    [InlineData(-20)]
    public Task NegativeOrZeroPageSize_ShouldReturn_400(int pageSize) =>
        api.Given()
            .When(GET, URI($"/api/products/?pageSize={pageSize}"))
            .Then(BAD_REQUEST);
}


public class GetProductsFixture(): ApiSpecification<Program>(new WarehouseTestWebApplicationFactory()), IAsyncLifetime
{
    public List<ProductListItem> RegisteredProducts { get; } = [];

    public async Task InitializeAsync()
    {
        var productsToRegister = new[]
        {
            new RegisterProductRequest("ZX1234", "ValidName", "ValidDescription"),
            new RegisterProductRequest("AD5678", "OtherValidName", "OtherValidDescription"),
            new RegisterProductRequest("BH90210", "AnotherValid", "AnotherValidDescription")
        };

        foreach (var registerProduct in productsToRegister)
        {
            var createdId = await Given()
                .When(POST, URI("/api/products"), BODY(registerProduct))
                .Then(CREATED)
                .GetCreatedId<Guid>();

            var (sku, name, _) = registerProduct;
            RegisteredProducts.Add(new ProductListItem(createdId, sku!, name!));
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

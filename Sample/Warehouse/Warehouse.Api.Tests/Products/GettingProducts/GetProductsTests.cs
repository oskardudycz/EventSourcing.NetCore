using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;
using Warehouse.Products.GettingProducts;
using Warehouse.Products.RegisteringProduct;
using Xunit;

namespace Warehouse.Api.Tests.Products.GettingProducts;

public class GetProductsTests: IClassFixture<GetProductsFixture>
{
    private readonly GetProductsFixture API;

    public GetProductsTests(GetProductsFixture api) =>
        API = api;

    [Fact]
    public Task ValidRequest_With_NoParams_ShouldReturn_200() =>
        API.Given(URI("/api/products/"))
            .When(GET)
            .Then(OK, RESPONSE_BODY(API.RegisteredProducts));

    [Fact]
    public Task ValidRequest_With_Filter_ShouldReturn_SubsetOfRecords()
    {
        var registeredProduct = API.RegisteredProducts.First();
        var filter = registeredProduct.Sku[1..];

        return API.Given(URI($"/api/products/?filter={filter}"))
            .When(GET)
            .Then(OK, RESPONSE_BODY(new List<ProductListItem> { registeredProduct }));
    }

    [Fact]
    public Task ValidRequest_With_Paging_ShouldReturn_PageOfRecords()
    {
        // Given
        const int page = 2;
        const int pageSize = 1;
        var pagedRecords = API.RegisteredProducts
            .Skip(page - 1)
            .Take(pageSize)
            .ToList();

        return API.Given(URI($"/api/products/?page={page}&pageSize={pageSize}"))
            .When(GET)
            .Then(OK, RESPONSE_BODY(pagedRecords));
    }

    [Fact]
    public Task NegativePage_ShouldReturn_400() =>
        API.Given(URI($"/api/products/?page={-20}"))
            .When(GET)
            .Then(BAD_REQUEST);

    [Theory]
    [InlineData(0)]
    [InlineData(-20)]
    public Task NegativeOrZeroPageSize_ShouldReturn_400(int pageSize) =>
        API.Given(URI($"/api/products/?pageSize={pageSize}"))
            .When(GET)
            .Then(BAD_REQUEST);
}


public class GetProductsFixture: ApiSpecification<Program>, IAsyncLifetime
{
    public List<ProductListItem> RegisteredProducts { get; } = new();

    public GetProductsFixture(): base(new WarehouseTestWebApplicationFactory()) { }

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
            var registerResponse = await Send(
                new ApiRequest(POST, URI("/api/products"), BODY(registerProduct))
            );

            await CREATED(registerResponse);

            var createdId = registerResponse.GetCreatedId<Guid>();

            var (sku, name, _) = registerProduct;
            RegisteredProducts.Add(new ProductListItem(createdId, sku!, name!));
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

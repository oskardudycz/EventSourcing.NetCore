using System.Net;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;
using Warehouse.Products.RegisteringProduct;
using Xunit;

namespace Warehouse.Api.Tests.Products.RegisteringProduct;

public class RegisterProductTests: IClassFixture<WarehouseTestWebApplicationFactory>
{
    private readonly ApiSpecification<Program> API;

    public RegisterProductTests(WarehouseTestWebApplicationFactory webApplicationFactory) =>
        API = ApiSpecification<Program>.Setup(webApplicationFactory);

    [Theory]
    [MemberData(nameof(ValidRequests))]
    public Task ValidRequest_ShouldReturn_201(RegisterProductRequest validRequest) =>
        API.Given()
            .When(
                POST,
                URI("/api/products/"),
                BODY(validRequest)
            )
            .Then(CREATED);

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public Task InvalidRequest_ShouldReturn_400(RegisterProductRequest invalidRequest) =>
        API.Given()
            .When(
                POST,
                URI("/api/products"),
                BODY(invalidRequest)
            )
            .Then(BAD_REQUEST);

    [Fact]
    public async Task RequestForExistingSKUShouldFail_ShouldReturn_409()
    {
        // Given
        var request = new RegisterProductRequest("AA2039485", ValidName, ValidDescription);

        // first one should succeed
        await API.Given()
            .When(
                POST,
                URI("/api/products/"),
                BODY(request)
            )
            .Then(CREATED);

        // second one will fail with conflict
        await API.Given()
            .When(
                POST,
                URI("/api/products/"),
                BODY(request)
            )
            .Then(HTTP_STATUS(HttpStatusCode.Forbidden));
    }

    private const string ValidName = "VALID_NAME";
    private static string ValidSKU => $"CC{DateTime.Now.Ticks}";
    private const string ValidDescription = "VALID_DESCRIPTION";

    public static TheoryData<RegisterProductRequest> ValidRequests =
    [
        new RegisterProductRequest(ValidSKU, ValidName, ValidDescription),
        new RegisterProductRequest(ValidSKU, ValidName, null)
    ];

    public static TheoryData<RegisterProductRequest> InvalidRequests =
    [
        new RegisterProductRequest(null, ValidName, ValidDescription),
        new RegisterProductRequest("INVALID_SKU", ValidName, ValidDescription),
        new RegisterProductRequest(ValidSKU, null, ValidDescription)
    ];
}

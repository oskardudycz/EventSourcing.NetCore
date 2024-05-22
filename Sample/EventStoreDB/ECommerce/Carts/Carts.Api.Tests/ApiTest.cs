using Core.Testing;
using Ogooreck.API;
using Xunit;

namespace Carts.Api.Tests;

public class ShoppingCartsApplicationFactory: TestWebApplicationFactory<Program>;

public class ApiFixture: IDisposable
{
    public ApiSpecification<Program> API { get; } =
        ApiSpecification<Program>.Setup(new ShoppingCartsApplicationFactory());

    public void Dispose() =>
        API.Dispose();
}

[CollectionDefinition("ApiTests")]
public class DatabaseCollection: ICollectionFixture<ApiFixture>;

[Collection("ApiTests")]
public abstract class ApiTest(ApiFixture fixture)
{
    protected readonly ApiSpecification<Program> API = fixture.API;
}

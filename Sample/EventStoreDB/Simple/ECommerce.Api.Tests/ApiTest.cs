using Core.Testing;
using Ogooreck.API;
using Xunit;

namespace ECommerce.Api.Tests;

public class ShoppingCartsApplicationFactory: TestWebApplicationFactory<Program>
{
    // protected override IHost CreateHost(IHostBuilder builder)
    // {
    //     var host = base.CreateHost(builder);
    //
    //     using var scope = host.Services.CreateScope();
    //     using var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
    //     var database = context.Database;
    //     database.ExecuteSqlRaw("TRUNCATE TABLE \"ShoppingCartDetailsProductItem\"");
    //     database.ExecuteSqlRaw("TRUNCATE TABLE \"ShoppingCartShortInfo\"");
    //     database.ExecuteSqlRaw("TRUNCATE TABLE \"ShoppingCartDetails\" CASCADE");
    //
    //     return host;
    // }
}

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

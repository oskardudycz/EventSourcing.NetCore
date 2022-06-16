using Core.Testing;
using ECommerce.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Warehouse.Api.Tests;

public class ShoppingCartsApplicationFactory: TestWebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        var database = context.Database;
        database.ExecuteSqlRaw("TRUNCATE TABLE \"ShoppingCartDetailsProductItem\"");
        database.ExecuteSqlRaw("TRUNCATE TABLE \"ShoppingCartShortInfo\"");
        database.ExecuteSqlRaw("TRUNCATE TABLE \"ShoppingCartDetails\" CASCADE");

        return host;
    }
}

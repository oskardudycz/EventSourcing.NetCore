using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Warehouse.Storage;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Warehouse.Api.Tests;

public class WarehouseTestWebApplicationFactory: WebApplicationFactory<Program>
{
    private readonly string schemaName = Guid.NewGuid().ToString("N").ToLower();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services
                .AddTransient(s =>
                {
                    var connectionString = s.GetRequiredService<IConfiguration>()
                        .GetConnectionString("WarehouseDB");
                    var options = new DbContextOptionsBuilder<WarehouseDBContext>();
                    options.UseNpgsql(
                        $"{connectionString}; searchpath = {schemaName.ToLower()}",
                        x => x.MigrationsHistoryTable("__EFMigrationsHistory", schemaName.ToLower()));
                    return options.Options;
                });
        });

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<WarehouseDBContext>().Database;
        database.ExecuteSqlRaw("TRUNCATE TABLE \"Product\"");

        return host;
    }
}

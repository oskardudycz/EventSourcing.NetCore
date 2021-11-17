using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Shipments.Storage;

internal class ShipmentsDbContextFactory: IDesignTimeDbContextFactory<ShipmentsDbContext>
{
    public ShipmentsDbContextFactory()
    {
    }

    public ShipmentsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ShipmentsDbContext>();

        if (optionsBuilder.IsConfigured)
            return new ShipmentsDbContext(optionsBuilder.Options);

        //Called by parameterless ctor Usually Migrations
        var environmentName = Environment.GetEnvironmentVariable("EnvironmentName") ?? "Development";

        var connectionString =
            new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build()
                .GetConnectionString("ShipmentsDatabase");

        optionsBuilder.UseNpgsql(connectionString);

        return new ShipmentsDbContext(optionsBuilder.Options);
    }
}
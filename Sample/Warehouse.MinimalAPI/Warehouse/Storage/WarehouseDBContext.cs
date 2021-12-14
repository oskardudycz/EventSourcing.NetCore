using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Warehouse.Products;

namespace Warehouse.Storage;

public class WarehouseDBContext: DbContext
{
    public WarehouseDBContext(DbContextOptions<WarehouseDBContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.SetupProductsModel();
    }
}

public class WarehouseDBContextFactory: IDesignTimeDbContextFactory<WarehouseDBContext>
{
    public WarehouseDBContext CreateDbContext(params string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WarehouseDBContext>();

        if (optionsBuilder.IsConfigured)
            return new WarehouseDBContext(optionsBuilder.Options);

        //Called by parameterless ctor Usually Migrations
        var environmentName = Environment.GetEnvironmentVariable("EnvironmentName") ?? "Development";

        var connectionString =
            new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build()
                .GetConnectionString("WarehouseDB");

        optionsBuilder.UseNpgsql(connectionString);

        return new WarehouseDBContext(optionsBuilder.Options);
    }

    public static WarehouseDBContext Create()
        => new WarehouseDBContextFactory().CreateDbContext();
}
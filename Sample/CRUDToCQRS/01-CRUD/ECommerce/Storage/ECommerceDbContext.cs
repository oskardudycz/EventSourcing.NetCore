namespace ECommerce.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class ECommerceDbContext(DbContextOptions<ECommerceDbContext> options): DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
}

public class ECommerceDbContextFactory: IDesignTimeDbContextFactory<ECommerceDbContext>
{
    public ECommerceDbContext CreateDbContext(params string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ECommerceDbContext>();

        if (optionsBuilder.IsConfigured)
            return new ECommerceDbContext(optionsBuilder.Options);

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

        return new ECommerceDbContext(optionsBuilder.Options);
    }

    public static ECommerceDbContext Create()
        => new ECommerceDbContextFactory().CreateDbContext();
}

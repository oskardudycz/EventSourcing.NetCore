using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Core.EntityFramework.Tests;

public class ShoppingCart
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public int ProductCount { get; set; }
}

public class TestDbContext(DbContextOptions<TestDbContext> options): DbContext(options)
{
    public DbSet<ShoppingCart> ShoppingCarts { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<ShoppingCart>();
}



public class TestDbContextFactory: IDesignTimeDbContextFactory<TestDbContext>
{
    public TestDbContext CreateDbContext(params string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();

        if (optionsBuilder.IsConfigured)
            return new TestDbContext(optionsBuilder.Options);

        //Called by parameterless ctor Usually Migrations
        var environmentName = Environment.GetEnvironmentVariable("EnvironmentName") ?? "Development";

        var connectionString =
            new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build()
                .GetConnectionString("TestDb");

        optionsBuilder.UseNpgsql(connectionString);

        return new TestDbContext(optionsBuilder.Options);
    }

    public static TestDbContext Create()
        => new TestDbContextFactory().CreateDbContext();
}


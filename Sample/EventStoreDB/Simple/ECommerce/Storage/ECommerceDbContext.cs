using ECommerce.ShoppingCarts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Storage;

public class ECommerceDbContext(DbContextOptions<ECommerceDbContext> options): DbContext(options), IDisposable, IAsyncDisposable
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.SetupShoppingCartsReadModels();
    }
}

public class ECommerceDBContextFactory: IDesignTimeDbContextFactory<ECommerceDbContext>
{
    public ECommerceDbContext CreateDbContext(params string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ECommerceDbContext>();

        if (optionsBuilder.IsConfigured)
            return new ECommerceDbContext(optionsBuilder.Options);

        optionsBuilder.UseNpgsql(
            "PORT = 5432; HOST = 127.0.0.1; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'");

        return new ECommerceDbContext(optionsBuilder.Options);
    }
}

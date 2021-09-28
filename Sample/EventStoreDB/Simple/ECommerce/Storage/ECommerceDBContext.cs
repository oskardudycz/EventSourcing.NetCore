using System;
using ECommerce.ShoppingCarts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Storage
{
    public class ECommerceDBContext: DbContext
    {
        public ECommerceDBContext(DbContextOptions<ECommerceDBContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetupShoppingCartsReadModels();
        }
    }

    public class ECommerceDBContextFactory: IDesignTimeDbContextFactory<ECommerceDBContext>
    {
        public ECommerceDBContext CreateDbContext(params string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ECommerceDBContext>();

            if (optionsBuilder.IsConfigured)
                return new ECommerceDBContext(optionsBuilder.Options);

            optionsBuilder.UseNpgsql(
                "PORT = 5432; HOST = 127.0.0.1; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'");

            return new ECommerceDBContext(optionsBuilder.Options);
        }
    }
}

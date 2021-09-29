using System;
using ECommerce.Core;
using ECommerce.Core.Entities;
using ECommerce.Core.Events;
using ECommerce.ShoppingCarts;
using ECommerce.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce
{
    public static class Configuration
    {
        public static IServiceCollection AddECommerceModule(this IServiceCollection services)
            => services
                .AddShoppingCartsModule()
                .AddDbContext<ECommerceDbContext>(
                    options => options.UseNpgsql("name=ConnectionStrings:ECommerceDB"))
                .AddSingleton<Func<Guid>>(Guid.NewGuid);

        public static void UseECommerceModule(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
            dbContext.Database.Migrate();
        }
    }
}

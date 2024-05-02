using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Products.Entity;
using ECommerce.Domain.Storage;

namespace ECommerce.Domain.Products.Repositories;

public class ProductRepository(ECommerceDbContext dbContext): Repository<Product>(dbContext);

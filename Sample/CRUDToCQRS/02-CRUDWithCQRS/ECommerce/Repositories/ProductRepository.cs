using ECommerce.Core.Repositories;
using ECommerce.Model;
using ECommerce.Storage;

namespace ECommerce.Repositories;

public class ProductRepository(ECommerceDbContext dbContext): Repository<Product>(dbContext);

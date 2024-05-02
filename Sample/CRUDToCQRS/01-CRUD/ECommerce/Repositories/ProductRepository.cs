using ECommerce.Core.Repositories;
using ECommerce.Model;
using ECommerce.Storage;

namespace ECommerce.Repositories;

public class ProductRepository(ECommerceDbContext dbContext): CRUDRepository<Product>(dbContext);

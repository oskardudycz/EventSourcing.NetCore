using ECommerce.Core.Repositories;
using ECommerce.Model;
using ECommerce.Storage;

namespace ECommerce.Repositories;

public class ProductRepository: CRUDRepository<Product>
{
    public ProductRepository(ECommerceDbContext dbContext) : base(dbContext)
    {
    }
}

using ECommerce.Core.Repositories;
using ECommerce.Storage;

namespace ECommerce.Domain.Products;

public class ProductRepository: CRUDRepository<ECommerceDbContext, Product>
{
    public ProductRepository(ECommerceDbContext dbContext) : base(dbContext)
    {
    }
}

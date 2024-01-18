using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Products.Entity;

namespace ECommerce.Domain.Products.Repositories;

public class ProductReadOnlyRepository: ReadOnlyRepository<Product>
{
    public ProductReadOnlyRepository(IQueryable<Product> query) : base(query)
    {
    }
}

using ECommerce.Core.Repositories;
using ECommerce.Model;
using ECommerce.Storage;

namespace ECommerce.Repositories;

public class ProductReadOnlyRepository: ReadOnlyRepository<Product>
{
    public ProductReadOnlyRepository(IQueryable<Product> query) : base(query)
    {
    }
}

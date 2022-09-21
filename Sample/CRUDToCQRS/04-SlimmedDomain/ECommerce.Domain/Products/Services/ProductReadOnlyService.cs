using AutoMapper;
using ECommerce.Domain.Core.Services;
using ECommerce.Domain.Products.Entity;

namespace ECommerce.Domain.Products.Services;

public class ProductReadOnlyService: ReadOnlyOnlyService<Product>
{
    public ProductReadOnlyService(IQueryable<Product> repository, IMapper mapper) : base(repository, mapper)
    {
    }
}

using AutoMapper;
using ECommerce.Domain.Core.Services;
using ECommerce.Domain.Products.Entity;
using ECommerce.Domain.Products.Repositories;

namespace ECommerce.Domain.Products.Services;

public class ProductReadOnlyService: ReadOnlyOnlyService<Product>
{
    public ProductReadOnlyService(ProductReadOnlyRepository repository, IMapper mapper) : base(repository, mapper)
    {
    }
}

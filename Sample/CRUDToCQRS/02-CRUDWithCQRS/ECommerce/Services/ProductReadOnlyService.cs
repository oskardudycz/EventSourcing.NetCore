using AutoMapper;
using ECommerce.Core.Services;
using ECommerce.Model;
using ECommerce.Repositories;

namespace ECommerce.Services;

public class ProductReadOnlyService: ReadOnlyOnlyService<Product>
{
    public ProductReadOnlyService(ProductReadOnlyRepository repository, IMapper mapper) : base(repository, mapper)
    {
    }
}

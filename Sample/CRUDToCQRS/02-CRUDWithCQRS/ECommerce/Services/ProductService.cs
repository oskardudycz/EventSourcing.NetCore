using AutoMapper;
using ECommerce.Core.Services;
using ECommerce.Model;
using ECommerce.Repositories;

namespace ECommerce.Services;

public class ProductService: Service<Product>
{
    public ProductService(ProductRepository repository, IMapper mapper) : base(repository, mapper)
    {
    }
}

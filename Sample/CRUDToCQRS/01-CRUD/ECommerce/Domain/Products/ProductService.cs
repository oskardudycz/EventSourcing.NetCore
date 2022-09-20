using AutoMapper;
using ECommerce.Core.Services;

namespace ECommerce.Domain.Products;

public class ProductService: CRUDService<ProductRepository, Product>
{
    public ProductService(ProductRepository repository, IMapper mapper) : base(repository, mapper)
    {
    }
}

using AutoMapper;
using ECommerce.Domain.Core.Services;
using ECommerce.Domain.Products.Entity;
using ECommerce.Domain.Storage;

namespace ECommerce.Domain.Products.Services;

public class ProductService: Service<Product>
{
    public ProductService(ECommerceDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
    }
}

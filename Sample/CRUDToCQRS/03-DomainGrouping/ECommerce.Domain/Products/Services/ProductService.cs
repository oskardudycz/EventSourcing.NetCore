using AutoMapper;
using ECommerce.Domain.Core.Services;
using ECommerce.Domain.Products.Entity;
using ECommerce.Domain.Products.Repositories;

namespace ECommerce.Domain.Products.Services;

public class ProductService(ProductRepository repository, IMapper mapper): Service<Product>(repository, mapper);

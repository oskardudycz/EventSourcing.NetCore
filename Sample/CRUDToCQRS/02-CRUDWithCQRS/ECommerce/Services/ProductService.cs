using AutoMapper;
using ECommerce.Core.Services;
using ECommerce.Model;
using ECommerce.Repositories;

namespace ECommerce.Services;

public class ProductService(ProductRepository repository, IMapper mapper): Service<Product>(repository, mapper);

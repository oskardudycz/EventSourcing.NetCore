using AutoMapper;
using ECommerce.Core.Services;
using ECommerce.Model;
using ECommerce.Repositories;

namespace ECommerce.Services;

public class ProductReadOnlyService(ProductReadOnlyRepository repository, IMapper mapper)
    : ReadOnlyOnlyService<Product>(repository, mapper);

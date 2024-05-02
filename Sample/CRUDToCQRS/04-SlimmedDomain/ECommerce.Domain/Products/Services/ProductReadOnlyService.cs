using AutoMapper;
using ECommerce.Domain.Core.Services;
using ECommerce.Domain.Products.Entity;

namespace ECommerce.Domain.Products.Services;

public class ProductReadOnlyService(IQueryable<Product> query, IMapper mapper)
    : ReadOnlyOnlyService<Product>(query, mapper);

using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Products.Entity;

namespace ECommerce.Domain.Products.Repositories;

public class ProductReadOnlyRepository(IQueryable<Product> query): ReadOnlyRepository<Product>(query);

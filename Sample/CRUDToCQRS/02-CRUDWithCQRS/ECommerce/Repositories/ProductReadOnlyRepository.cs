using ECommerce.Core.Repositories;
using ECommerce.Model;

namespace ECommerce.Repositories;

public class ProductReadOnlyRepository(IQueryable<Product> query): ReadOnlyRepository<Product>(query);

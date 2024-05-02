using ECommerce.Core.Repositories;
using ECommerce.Model;
using ECommerce.Storage;

namespace ECommerce.Repositories;

public class ProductReadOnlyRepository(IQueryable<Product> query): ReadOnlyRepository<Product>(query);

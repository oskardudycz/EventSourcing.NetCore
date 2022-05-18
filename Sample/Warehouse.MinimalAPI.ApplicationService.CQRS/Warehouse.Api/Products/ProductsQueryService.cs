using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Storage;

namespace Warehouse.Api.Products;

public interface IProductsQueryService
{
    Task<List<Product>> GetAll(
        string? filter,
        int? page,
        int? pageSize,
        CancellationToken ct
    );

    Task<Product?> GetById(Guid id, CancellationToken ct);
}

public class ProductsQueryService: IProductsQueryService
{
    private readonly IQueryable<Product> products;

    public ProductsQueryService(WarehouseDBContext dbContext)
    {
        products = dbContext.Set<Product>().AsNoTracking();
    }

    public Task<List<Product>> GetAll(
        string? filter,
        int? page,
        int? pageSize,
        CancellationToken ct
    )
    {
        var take = pageSize ?? 0;
        var pageNumber = page ?? 1;

        var filteredProducts = string.IsNullOrEmpty(filter)
            ? products
            : products
                .Where(p =>
                    p.Sku.Contains(filter) ||
                    p.Name.Contains(filter) ||
                    p.Description!.Contains(filter)
                );

        return filteredProducts
            .Skip(take * (pageNumber - 1))
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<Product?> GetById(Guid id, CancellationToken ct)
    {
        return products.SingleOrDefaultAsync(p => p.Id == id, ct);
    }
}

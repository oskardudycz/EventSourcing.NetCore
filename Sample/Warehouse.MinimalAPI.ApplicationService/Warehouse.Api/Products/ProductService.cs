using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Products.Requests;
using Warehouse.Api.Storage;

namespace Warehouse.Api.Products;

public class ProductService: IProductService
{
    private readonly WarehouseDBContext dbContext;

    public ProductService(WarehouseDBContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task Add(Product product, CancellationToken ct)
    {
        var products = dbContext.Set<Product>();

        if (await products.AnyAsync(p => p.Sku == product.Sku, ct))
        {
            throw new InvalidOperationException(
                $"Product with SKU `{product.Sku} already exists.");
        }

        await products.AddAsync(product, ct);
        await dbContext.SaveChangesAsync(ct);
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

        var products = dbContext.Set<Product>();

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
        var products = dbContext.Set<Product>();

        return products.SingleOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task Update(UpdateProduct request, CancellationToken ct)
    {
        // VALIDATION
        if (request.Id == Guid.Empty || string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentOutOfRangeException(nameof(request), "Invalid Request");

        // GETTING DATA
        var product = await GetById(request.Id, ct);

        // BUSINESS VALIDATION
        if (product == null)
            throw new Exception($"Product with id: {request.Id} was not found");

        var products = dbContext.Set<Product>();

        if (await products.AnyAsync(p => p.Sku == request.Sku, ct))
        {
            throw new InvalidOperationException(
                $"Product with SKU `{request.Sku} already exists.");
        }

        // MAPPING
        product.Name = request.Name;
        product.Description = request.Description;
        product.Sku = request.Sku;

        // SAVE CHANGES
        products.Update(product);
        await dbContext.SaveChangesAsync(ct);
    }
}

using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Products.Requests;
using Warehouse.Api.Storage;

namespace Warehouse.Api.Products;

public class ProductService: IProductService
{
    private readonly DbSet<Product> products;
    private readonly Func<CancellationToken, Task<int>> SaveChangesAsync;

    public ProductService(WarehouseDBContext dbContext)
    {
        products = dbContext.Set<Product>();
        SaveChangesAsync = dbContext.SaveChangesAsync;
    }

    public async Task Add(Product product, CancellationToken ct)
    {
        if (await products.AnyAsync(p => p.Sku == product.Sku, ct))
        {
            throw new InvalidOperationException(
                $"Product with SKU `{product.Sku} already exists.");
        }

        await products.AddAsync(product, ct);
        await SaveChangesAsync(ct);
    }

    public async Task Update(UpdateProduct request, CancellationToken ct)
    {
        // VALIDATION
        if (request.Id == Guid.Empty || string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentOutOfRangeException(nameof(request), "Invalid Request");

        // GETTING DATA
        var product = await products.FindAsync(new object[] { request.Id }, cancellationToken:ct);

        // BUSINESS VALIDATION
        if (product == null)
            throw new Exception($"Product with id: {request.Id} was not found");

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
        await SaveChangesAsync(ct);
    }
}

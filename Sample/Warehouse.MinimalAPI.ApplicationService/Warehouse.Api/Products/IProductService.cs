using Warehouse.Api.Products.Requests;

namespace Warehouse.Api.Products;

public interface IProductService
{
    Task Add(Product product, CancellationToken ct);

    Task<List<Product>> GetAll(
        string? filter,
        int? page,
        int? pageSize,
        CancellationToken ct
    );

    Task<Product?> GetById(Guid id, CancellationToken ct);
    Task Update(UpdateProduct request, CancellationToken ct);
}

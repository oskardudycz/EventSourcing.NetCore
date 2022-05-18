using Warehouse.Api.Products.Requests;

namespace Warehouse.Api.Products;

public interface IProductService
{
    Task Add(Product product, CancellationToken ct);
    Task Update(UpdateProduct request, CancellationToken ct);
}

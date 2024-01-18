using ECommerce.Domain.Products.GettingById;
using ECommerce.Domain.Products.GettingProducts;

namespace ECommerce.Domain.Products;

using static GetProductByIdHandler;
using static GetProductsHandler;

public class ProductReadOnlyService
{
    private readonly IQueryable<Product> products;

    public ProductReadOnlyService(IQueryable<Product> products) => this.products = products;

    public Task<ProductDetailsResponse?> GetByIdAsync(GetProductById query, CancellationToken ct) =>
        products.HandleAsync(query, ct);

    public Task<List<ProductShortInfoResponse>> GetPagedAsync(
        GetProducts query,
        CancellationToken ct
    ) =>
        products.HandleAsync(query, ct);
}

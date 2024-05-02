using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Core.Primitives;
using Warehouse.Api.Core.Queries;

namespace Warehouse.Api.Products;

internal class HandleGetProductDetails(IQueryable<Product> products): IQueryHandler<GetProductDetails, ProductDetails?>
{
    public async ValueTask<ProductDetails?> Handle(GetProductDetails query, CancellationToken ct)
    {
        var product = await products
            .SingleOrDefaultAsync(p => p.Id == query.ProductId, ct);

        if (product == null)
            return null;

        return new ProductDetails(
            product.Id,
            product.Sku.Value,
            product.Name,
            product.Description
        );
    }
}

public record GetProductDetails(
    Guid ProductId
)
{
    public static GetProductDetails With(Guid? productId)
        => new(productId.AssertNotEmpty(nameof(productId)));
}

public record ProductDetails(
    Guid Id,
    string Sku,
    string Name,
    string? Description
);

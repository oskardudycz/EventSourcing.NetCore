using ECommerce.Domain.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Domain.Products.GettingProducts;

public record ProductShortInfoResponse(
    Guid Id,
    string Sku,
    string Name
);

public record GetProducts(
    int PageNumber = 1,
    int PageSize = 20
);

public static class GetProductsHandler
{
    public static Task<List<ProductShortInfoResponse>> HandleAsync(
        this IQueryable<Product> products,
        GetProducts query,
        CancellationToken ct
    )
    {
        var (pageNumber, pageSize) = query;

        return products.GetPage(pageNumber, pageSize)
            .Select(p => new ProductShortInfoResponse(p.Id, p.Sku, p.Name))
            .ToListAsync(ct);
    }
}

using Microsoft.EntityFrameworkCore;

namespace ECommerce.Domain.Products.GettingById;

public record ProductDetailsResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string ProducerName,
    string? AdditionalInfo
);

public record GetProductById(
    Guid Id
);

public static class GetProductByIdHandler
{
    public static Task<ProductDetailsResponse?> HandleAsync(
        this IQueryable<Product> products,
        GetProductById query,
        CancellationToken ct
    ) =>
        products
            .Select(p =>
                new ProductDetailsResponse(p.Id, p.Sku, p.Name, p.Description, p.ProducerName, p.AdditionalInfo)
            )
            .SingleOrDefaultAsync(p => p.Id == query.Id, ct);
}

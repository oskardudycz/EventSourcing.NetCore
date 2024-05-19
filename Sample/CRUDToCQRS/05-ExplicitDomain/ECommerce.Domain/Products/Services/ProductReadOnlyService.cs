using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Products.Contracts.Responses;
using ECommerce.Domain.Products.Entity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Domain.Products.Services;

public class ProductReadOnlyService(IQueryable<Product> query)
{
    public Task<ProductDetailsResponse?> GetByIdAsync(Guid id, CancellationToken ct) =>
        query
            .Select(p =>
                new ProductDetailsResponse(p.Id, p.Sku, p.Name, p.Description, p.ProducerName, p.AdditionalInfo)
            )
            .SingleOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<ProductShortInfoResponse>> GetPagedAsync(
        CancellationToken ct,
        int pageNumber = 1,
        int pageSize = 20
    ) =>
        query.GetPage(pageNumber, pageSize)
            .Select(p => new ProductShortInfoResponse(p.Id, p.Sku, p.Name))
            .ToListAsync(ct);
}

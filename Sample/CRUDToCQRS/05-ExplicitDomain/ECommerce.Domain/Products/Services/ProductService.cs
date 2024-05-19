using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Products.Contracts.Requests;
using ECommerce.Domain.Products.Entity;
using ECommerce.Domain.Storage;

namespace ECommerce.Domain.Products.Services;

public class ProductService(ECommerceDbContext dbContext)
{
    public Task CreateAsync(
        CreateProductRequest command,
        CancellationToken ct
    ) =>
        dbContext.AddAndSaveChanges(
            new Product
            {
                Id = command.Id,
                Sku = command.Sku,
                Name = command.Name,
                Description = command.Description,
                AdditionalInfo = command.AdditionalInfo,
                ProducerName = command.ProducerName
            },
            ct
        );

    public async Task UpdateAsync(
        UpdateProductRequest request,
        CancellationToken ct
    ) =>
        await dbContext.UpdateAndSaveChanges<Product>(
            request.Id,
            product =>
            {
                product.Name = request.Name;
                product.Description = request.Description;
                product.AdditionalInfo = request.AdditionalInfo;
                product.ProducerName = request.ProducerName;

                return product;
            },
            ct
        );

    public Task DeleteByIdAsync(Guid id, CancellationToken ct) =>
        dbContext.DeleteAndSaveChanges<Product>(id, ct);
}

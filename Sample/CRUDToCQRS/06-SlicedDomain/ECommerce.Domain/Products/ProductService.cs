using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Products.CreatingProduct;
using ECommerce.Domain.Products.UpdatingProduct;
using ECommerce.Domain.Storage;

namespace ECommerce.Domain.Products;

using static CreateProductHandler;
using static UpdateProductHandler;

public class ProductService(ECommerceDbContext dbContext)
{
    public Task CreateAsync(CreateProduct command, CancellationToken ct) =>
        dbContext.AddAndSaveChanges(
            Handle(command),
            ct
        );

    public Task UpdateAsync(UpdateProduct command, CancellationToken ct) =>
        dbContext.UpdateAndSaveChanges<Product>(
            command.Id,
            product => Handle(product, command),
            ct
        );

    public Task DeleteByIdAsync(Guid id, CancellationToken ct) =>
        dbContext.DeleteAndSaveChanges<Product>(id, ct);
}

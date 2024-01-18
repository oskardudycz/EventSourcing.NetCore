using ECommerce.Domain.Products.CreatingProduct;
using ECommerce.Domain.Products.UpdatingProduct;

namespace ECommerce.Contracts.Requests;

public record CreateProductRequest(
    string? Sku,
    string? Name,
    string? Description,
    string? ProducerName,
    string? AdditionalInfo
)
{
    public CreateProduct ToCommand(Guid id) =>
        new CreateProduct(
            id,
            Sku ?? throw new ArgumentNullException(nameof(Sku)),
            Name ?? throw new ArgumentNullException(nameof(Name)),
            Description,
            ProducerName ?? throw new ArgumentNullException(nameof(ProducerName)),
            AdditionalInfo
        );
}

public record UpdateProductRequest(
    string? Name,
    string? Description,
    string? ProducerName,
    string? AdditionalInfo
)
{
    public UpdateProduct ToCommand(Guid id) =>
        new UpdateProduct(
            id,
            Name ?? throw new ArgumentNullException(nameof(Name)),
            Description,
            ProducerName ?? throw new ArgumentNullException(nameof(ProducerName)),
            AdditionalInfo
        );
}

using ECommerce.Domain.Core.Requests;

namespace ECommerce.Domain.Products.Contracts.Requests;

public record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    string ProducerName,
    string? AdditionalInfo
): ICreateRequest
{
    public Guid Id { get; set; }
}

using ECommerce.Domain.Core.Requests;

namespace ECommerce.Domain.Products.Contracts.Requests;

public record UpdateProductRequest(
    string Name,
    string? Description,
    string ProducerName,
    string? AdditionalInfo
): IUpdateRequest
{
    public Guid Id { get; set; }
};

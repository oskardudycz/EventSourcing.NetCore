namespace ECommerce.Domain.Products.Contracts.Responses;

public record ProductDetailsResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string ProducerName,
    string? AdditionalInfo
);

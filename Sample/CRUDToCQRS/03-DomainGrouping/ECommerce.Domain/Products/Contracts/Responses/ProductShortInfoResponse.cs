namespace ECommerce.Domain.Products.Contracts.Responses;

public record ProductShortInfoResponse(
    Guid Id,
    string Sku,
    string Name
);

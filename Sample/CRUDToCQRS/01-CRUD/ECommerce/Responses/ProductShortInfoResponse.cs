namespace ECommerce.Responses;

public record ProductShortInfoResponse(
    Guid Id,
    string Sku,
    string Name
);

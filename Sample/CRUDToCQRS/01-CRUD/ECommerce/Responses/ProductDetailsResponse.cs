using ECommerce.Core.Responses;

namespace ECommerce.Responses;

public record ProductShortInfoResponse(
    Guid Id,
    string Sku,
    string Name
);

public record ProductDetailsResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string ProducerName,
    string? AdditionalInfo
): ICreatedResponse;

using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Requests;

public record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    string ProducerName,
    string? AdditionalInfo
);

public record UpdateProductRequest(
    [FromRoute]
    Guid Id,
    string Name,
    string? Description,
    string ProducerName,
    string? AdditionalInfo
);

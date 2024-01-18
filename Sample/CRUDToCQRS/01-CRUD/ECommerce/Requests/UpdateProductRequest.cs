using ECommerce.Core.Requests;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Requests;

public record UpdateProductRequest(
    [FromRoute]
    Guid Id,
    string Name,
    string? Description,
    string ProducerName,
    string? AdditionalInfo
): IUpdateRequest;

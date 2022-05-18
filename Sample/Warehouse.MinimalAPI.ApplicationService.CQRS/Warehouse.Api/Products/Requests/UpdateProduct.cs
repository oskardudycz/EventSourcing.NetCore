namespace Warehouse.Api.Products.Requests;

public record UpdateProduct(Guid Id, string Sku, string Name, string? Description);

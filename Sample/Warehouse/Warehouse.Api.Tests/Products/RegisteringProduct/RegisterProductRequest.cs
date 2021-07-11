namespace Warehouse.Api.Tests.Products.RegisteringProduct
{
    public record RegisterProductRequest(
        string? SKU,
        string? Name,
        string? Description
    );
}

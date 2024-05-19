namespace ECommerce.Domain.Products.CreatingProduct;

public record CreateProduct(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string ProducerName,
    string? AdditionalInfo
);

public static class CreateProductHandler
{
    public static Product Handle(CreateProduct command) =>
        new()
        {
            Id = command.Id,
            Sku = command.Sku,
            Name = command.Name,
            Description = command.Description,
            AdditionalInfo = command.AdditionalInfo,
            ProducerName = command.ProducerName
        };
}

namespace ECommerce.Domain.Products.UpdatingProduct;

public record UpdateProduct(
    Guid Id,
    string Name,
    string? Description,
    string ProducerName,
    string? AdditionalInfo
);

public static class UpdateProductHandler
{
    public static Product Handle(Product product, UpdateProduct command)
    {
        product.Name = command.Name;
        product.Description = command.Description;
        product.AdditionalInfo = command.AdditionalInfo;
        product.ProducerName = command.ProducerName;

        return product;
    }
}

namespace ECommerce.Domain.Products.Entity;

public class Product
{
    public Guid Id { get; set; } = default;
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string ProducerName { get; set; } = default!;
    public string? AdditionalInfo { get; set; }
}

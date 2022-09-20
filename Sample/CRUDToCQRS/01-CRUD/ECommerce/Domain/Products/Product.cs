using ECommerce.Core;

namespace ECommerce.Domain.Products;

public class Product: IEntity
{
    public Guid Id { get; set; } = default;
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string ProducerName { get; set; } = default!;
    public string? AdditionalInfo { get; set; }
}

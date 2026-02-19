using ECommerce.Domain.Core.Entities;

namespace ECommerce.Domain.Products.Entity;

public class Product: IEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Sku { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string ProducerName { get; set; } = null!;
    public string? AdditionalInfo { get; set; }
}

using Shipments.Products;

namespace Shipments.Packages;

public class Package
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }

    public List<ProductItem> ProductItems { get; set; } = null!;

    public DateTime SentAt { get; set; }
}
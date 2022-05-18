namespace Warehouse.Api.Products;

public class Product
{
    public Product(Guid id, string sku, string name, string? description)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Description = description;
    }

    public Guid Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
}

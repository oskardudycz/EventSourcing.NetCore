using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Warehouse.Api.Products;

internal class Product
{
    public Guid Id { get; set; }

    /// <summary>
    /// The Stock Keeping Unit (SKU), i.e. a merchant-specific identifier for a product or service, or the product to which the offer refers.
    /// </summary>
    /// <returns></returns>
    public SKU Sku { get; set; } = null!;

    /// <summary>
    /// Product Name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Optional Product description
    /// </summary>
    public string? Description { get; set; }

    private Product(){}

    public Product(Guid id, SKU sku, string name, string? description)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Description = description;
    }
}

[method: JsonConstructor]
public record SKU(string Value)
{
    public static SKU Create(string? value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(SKU));
        if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, "[A-Z]{2,4}[0-9]{4,18}"))
            throw new ArgumentOutOfRangeException(nameof(SKU));

        return new SKU(value);
    }
}

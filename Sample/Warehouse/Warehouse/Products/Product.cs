using System;
using Warehouse.Products.Primitives;

namespace Warehouse.Products;

internal class Product
{
    public Guid Id { get; set; }

    /// <summary>
    /// The Stock Keeping Unit (SKU), i.e. a merchant-specific identifier for a product or service, or the product to which the offer refers.
    /// </summary>
    /// <returns></returns>
    public SKU Sku { get; set; } = default!;

    /// <summary>
    /// Product Name
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Optional Product description
    /// </summary>
    public string? Description { get; set; }

    // Note: this is needed because we're using SKU DTO.
    // It would work if we had just primitives
    // Should be fixed in .NET 6
    private Product(){}

    public Product(Guid id, SKU sku, string name, string? description)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Description = description;
    }
}
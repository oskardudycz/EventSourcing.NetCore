using System;
using System.Threading;
using System.Threading.Tasks;
using Warehouse.Core.Commands;
using Warehouse.Products.Primitives;

namespace Warehouse.Products.RegisteringProduct;

internal class HandleRegisterProduct : ICommandHandler<RegisterProduct>
{
    private readonly Func<Product, CancellationToken, ValueTask> addProduct;
    private readonly Func<SKU, CancellationToken, ValueTask<bool>> productWithSKUExists;

    public HandleRegisterProduct(
        Func<Product, CancellationToken, ValueTask> addProduct,
        Func<SKU, CancellationToken, ValueTask<bool>> productWithSKUExists
    )
    {
        this.addProduct = addProduct;
        this.productWithSKUExists = productWithSKUExists;
    }

    public async ValueTask Handle(RegisterProduct command, CancellationToken ct)
    {
        var product = new Product(
            command.ProductId,
            command.SKU,
            command.Name,
            command.Description
        );

        if (await productWithSKUExists(command.SKU, ct))
            throw new InvalidOperationException(
                $"Product with SKU `{command.SKU} already exists.");

        await addProduct(product, ct);
    }
}

public record RegisterProduct
{
    public Guid ProductId { get;}

    public SKU SKU { get; }

    public string Name { get; }

    public string? Description { get; }

    private RegisterProduct(Guid productId, SKU sku, string name, string? description)
    {
        ProductId = productId;
        SKU = sku;
        Name = name;
        Description = description;
    }

    public static RegisterProduct Create(Guid? id, string? sku, string? name, string? description)
    {
        if (!id.HasValue || id == Guid.Empty) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrEmpty(sku)) throw new ArgumentOutOfRangeException(nameof(sku));
        if (string.IsNullOrEmpty(name)) throw new ArgumentOutOfRangeException(nameof(name));
        if (description is "") throw new ArgumentOutOfRangeException(nameof(name));

        return new RegisterProduct(id.Value, SKU.Create(sku), name, description);
    }
}
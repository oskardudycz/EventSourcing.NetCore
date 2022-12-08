using Warehouse.Api.Core.Commands;

namespace Warehouse.Api.Products;

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

    public async Task Handle(RegisterProduct command, CancellationToken ct)
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

public record RegisterProduct(
    Guid ProductId,
    SKU SKU,
    string Name,
    string? Description
)
{
    public static RegisterProduct With(Guid? id, string? sku, string? name, string? description)
    {
        if (!id.HasValue || id == Guid.Empty) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrEmpty(sku)) throw new ArgumentOutOfRangeException(nameof(sku));
        if (string.IsNullOrEmpty(name)) throw new ArgumentOutOfRangeException(nameof(name));
        if (description is "") throw new ArgumentOutOfRangeException(nameof(name));

        return new RegisterProduct(id.Value, SKU.Create(sku), name, description);
    }
}

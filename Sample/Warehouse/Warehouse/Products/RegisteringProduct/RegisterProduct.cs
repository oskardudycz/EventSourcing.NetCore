using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Warehouse.Core.Commands;
using Warehouse.Core.Primitives;
using Warehouse.Products.Primitives;

namespace Warehouse.Products.RegisteringProduct
{
    internal class HandleRegisterProduct: ICommandHandler<RegisterProduct>
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

        public async ValueTask<CommandResult> Handle(RegisterProduct command, CancellationToken ct)
        {
            var productId = Guid.NewGuid();
            var (skuValue, name, description) = command;

            var sku = SKU.Create(skuValue);

            var product = new Product(
                productId,
                sku,
                name,
                description
            );

            if (await productWithSKUExists(sku, ct))
                throw new InvalidOperationException(
                    $"Product with SKU `{command.Sku} already exists.");

            await addProduct(product, ct);

            return CommandResult.Of(productId);
        }
    }

    public record RegisterProduct
    {
        public string Sku { get; }

        public string Name { get; }

        public string? Description { get; }

        [JsonConstructor]
        public RegisterProduct(string? sku, string? name, string? description)
        {
            Sku = sku.AssertNotEmpty();
            Name = name.AssertNotEmpty();
            Description = description;
        }

        public void Deconstruct(out string sku, out string name, out string? description)
        {
            sku = Sku;
            name = Name;
            description = Description;
        }
    }
}

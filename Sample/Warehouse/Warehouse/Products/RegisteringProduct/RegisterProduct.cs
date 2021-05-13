using System;
using System.Threading;
using System.Threading.Tasks;
using Warehouse.Core.Commands;
using Warehouse.Products.Primitives;

namespace Warehouse.Products.RegisteringProduct
{
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
                throw new ArgumentOutOfRangeException(
                    nameof(command.SKU),
                    $"Product with SKU `{command.SKU} already exists.");

            await addProduct(product, ct);
        }
    }

    public record RegisterProduct
    {
        public Guid ProductId { get;}

        /// <summary>
        /// The Stock Keeping Unit (SKU), i.e. a merchant-specific identifier for a product or service, or the product to which the offer refers.
        /// </summary>
        /// <returns></returns>
        public SKU SKU { get; }

        /// <summary>
        /// The area where a cashier works
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Current cashier working on the cash register
        /// </summary>
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
            if (!id.HasValue) throw new ArgumentNullException(nameof(id));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (description == null) throw new ArgumentNullException(nameof(description));

            return new RegisterProduct(id.Value, SKU.Create(sku), name, description);
        }
    }
}

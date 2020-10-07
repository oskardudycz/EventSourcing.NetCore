using System;
using Ardalis.GuardClauses;

namespace Carts.Carts.ValueObjects
{
    public class ProductItem
    {
        public Guid ProductId { get; }

        public int Quantity { get; }

        private ProductItem(Guid productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public ProductItem Create(Guid productId, int quantity)
        {
            Guard.Against.Default(productId, nameof(productId));
            Guard.Against.NegativeOrZero(quantity, nameof(quantity));

            return new ProductItem(productId, quantity);
        }

        public ProductItem MergeWith(ProductItem productItem)
        {
            if (!MatchesProduct(productItem))
                throw new ArgumentException("Product does not match.");

            return Create(ProductId, Quantity + productItem.Quantity);
        }

        public ProductItem Substract(ProductItem productItem)
        {
            if (!MatchesProduct(productItem))
                throw new ArgumentException("Product does not match.");

            return Create(ProductId, Quantity - productItem.Quantity);
        }

        public bool MatchesProduct(ProductItem productItem)
        {
            return ProductId == productItem.ProductId;
        }

        public bool HasEnough(int quantity)
        {
            return Quantity >= quantity;
        }

        public bool HasTheSameQuantity(ProductItem productItem)
        {
            return Quantity == productItem.Quantity;
        }
    }
}

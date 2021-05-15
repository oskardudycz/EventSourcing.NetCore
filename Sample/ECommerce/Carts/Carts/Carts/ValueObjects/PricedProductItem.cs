using System;

namespace Carts.Carts.ValueObjects
{
    public class PricedProductItem
    {
        public Guid ProductId => ProductItem.ProductId;

        public int Quantity => ProductItem.Quantity;

        public decimal UnitPrice { get; }

        public decimal TotalPrice => Quantity * UnitPrice;
        public ProductItem ProductItem { get; }

        private PricedProductItem(ProductItem productItem, decimal unitPrice)
        {
            ProductItem = productItem;
            UnitPrice = unitPrice;
        }

        public static PricedProductItem Create(Guid? productId, int? quantity, decimal? unitPrice)
        {
            return Create(
                ProductItem.Create(productId, quantity),
                unitPrice
            );
        }

        public static PricedProductItem Create(ProductItem productItem, decimal? unitPrice)
        {
            return unitPrice switch
            {
                null => throw new ArgumentNullException(nameof(unitPrice)),
                <= 0 => throw new ArgumentOutOfRangeException(nameof(unitPrice),
                    "Unit price has to be positive number"),
                _ => new PricedProductItem(productItem, unitPrice.Value)
            };
        }

        public bool MatchesProductAndPrice(PricedProductItem pricedProductItem)
        {
            return ProductId == pricedProductItem.ProductId && UnitPrice == pricedProductItem.UnitPrice;
        }

        public PricedProductItem MergeWith(PricedProductItem pricedProductItem)
        {
            if (!MatchesProductAndPrice(pricedProductItem))
                throw new ArgumentException("Product or price does not match.");

            return new PricedProductItem(ProductItem.MergeWith(pricedProductItem.ProductItem), UnitPrice);
        }

        public PricedProductItem Substract(PricedProductItem pricedProductItem)
        {
            if (!MatchesProductAndPrice(pricedProductItem))
                throw new ArgumentException("Product or price does not match.");

            return new PricedProductItem(ProductItem.Substract(pricedProductItem.ProductItem), UnitPrice);
        }

        public bool HasEnough(int quantity)
        {
            return ProductItem.HasEnough(quantity);
        }

        public bool HasTheSameQuantity(PricedProductItem pricedProductItem)
        {
            return ProductItem.HasTheSameQuantity(pricedProductItem.ProductItem);
        }
    }
}

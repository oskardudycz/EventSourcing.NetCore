using System;

namespace Carts.Carts.ValueObjects
{
    public class PricedProductItem
    {
        public Guid ProductId => ProductItem.ProductId;

        public int Quantity => ProductItem.Quantity;

        public decimal UnitPrice { get; }
        public ProductItem ProductItem { get; }

        public PricedProductItem(ProductItem productItem, decimal unitPrice)
        {
            ProductItem = productItem;
            UnitPrice = unitPrice;
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

using System;

namespace Carts.Carts.ValueObjects
{
    public class PricedProductItem
    {
        public ProductItem ProductItem { get; }

        public Guid ProductId => ProductItem.ProductId;

        public int Quantity  => ProductItem.Quantity;

        public decimal UnitPrice { get; }

        public PricedProductItem(ProductItem productItem, decimal unitPrice)
        {
            ProductItem = productItem;
            UnitPrice = unitPrice;
        }

        public bool MatchesProductAndPrice(PricedProductItem pricedProductItem)
        {
            return ProductId == pricedProductItem.ProductId && UnitPrice == pricedProductItem.UnitPrice;
        }

        public PricedProductItem SumQuantity(PricedProductItem pricedProductItem)
        {
            if (!MatchesProductAndPrice(pricedProductItem))
                throw new ArgumentException("Product or price does not match.");

            return new PricedProductItem(ProductItem.SumQuantity(pricedProductItem.ProductItem), UnitPrice);
        }
    }
}

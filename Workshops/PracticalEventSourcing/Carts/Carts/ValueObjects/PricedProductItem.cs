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
    }
}

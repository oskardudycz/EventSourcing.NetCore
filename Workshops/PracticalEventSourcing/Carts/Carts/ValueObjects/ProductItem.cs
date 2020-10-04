using System;

namespace Carts.Carts.ValueObjects
{
    public class ProductItem
    {
        public Guid ProductId { get; }

        public decimal UnitPrice { get; }

        public int Quantity { get; }

        public ProductItem(Guid productId, decimal unitPrice, int quantity)
        {
            ProductId = productId;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }
    }
}

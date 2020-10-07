using System;

namespace Orders.Products.ValueObjects
{
    public class PricedProductItem
    {
        public Guid ProductId { get; }
        public int Quantity { get; }
        public decimal UnitPrice { get; }

        public PricedProductItem(Guid productId, int quantity, decimal unitPrice)
        {
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }
}

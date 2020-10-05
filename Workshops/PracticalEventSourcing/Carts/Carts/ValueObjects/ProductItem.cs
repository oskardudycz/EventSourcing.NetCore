using System;

namespace Carts.Carts.ValueObjects
{
    public class ProductItem
    {
        public Guid ProductId { get; }

        public int Quantity { get; }

        public ProductItem(Guid productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }
    }
}

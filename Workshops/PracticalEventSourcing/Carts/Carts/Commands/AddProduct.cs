using System;
using Carts.Carts.ValueObjects;
using Core.Commands;

namespace Carts.Carts.Commands
{
    public class AddProduct: ICommand
    {
        public Guid CartId { get; }

        public ProductItem ProductItem { get; }

        public AddProduct(Guid cartId, ProductItem productItem)
        {
            CartId = cartId;
            ProductItem = productItem;
        }
    }
}

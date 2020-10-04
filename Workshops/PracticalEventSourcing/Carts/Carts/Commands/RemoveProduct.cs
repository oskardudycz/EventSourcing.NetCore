using System;
using Carts.Carts.ValueObjects;
using Core.Commands;

namespace Carts.Carts.Commands
{
    public class RemoveProduct: ICommand
    {
        public Guid CartId { get; }

        public ProductItem ProductItem { get; }

        public RemoveProduct(Guid cardId, ProductItem productItem)
        {
            CartId = cardId;
            ProductItem = productItem;
        }
    }
}

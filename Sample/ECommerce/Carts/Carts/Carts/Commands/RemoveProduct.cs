using System;
using Carts.Carts.ValueObjects;
using Core.Commands;

namespace Carts.Carts.Commands
{
    public class RemoveProduct: ICommand
    {
        public Guid CartId { get; }

        public PricedProductItem ProductItem { get; }

        private RemoveProduct(Guid cardId, PricedProductItem productItem)
        {
            CartId = cardId;
            ProductItem = productItem;
        }

        public static RemoveProduct Create(Guid cardId, PricedProductItem productItem)
        {
            return new RemoveProduct(cardId, productItem);
        }
    }
}

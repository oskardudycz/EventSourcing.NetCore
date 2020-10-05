using System;
using System.Collections.Generic;
using Carts.Carts.ValueObjects;
using Core.Commands;

namespace Orders.Orders.Commands
{
    public class InitOrder: ICommand
    {
        public Guid ClientId { get; }

        public IReadOnlyList<PricedProductItem> ProductItems { get; }

        public decimal TotalPrice { get; }

        public InitOrder(Guid clientId, IReadOnlyList<PricedProductItem> productItems, decimal totalPrice)
        {
            ClientId = clientId;
            ProductItems = productItems;
            TotalPrice = totalPrice;
        }
    }
}

using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using Core.Commands;
using Orders.Products;
using Orders.Products.ValueObjects;

namespace Orders.Orders.Commands
{
    public class InitOrder: ICommand
    {
        public Guid OrderId { get; }
        public Guid ClientId { get; }

        public IReadOnlyList<PricedProductItem> ProductItems { get; }

        public decimal TotalPrice { get; }

        private InitOrder(
            Guid orderId,
            Guid clientId,
            IReadOnlyList<PricedProductItem> productItems,
            decimal totalPrice)
        {
            OrderId = orderId;
            ClientId = clientId;
            ProductItems = productItems;
            TotalPrice = totalPrice;
        }

        public static InitOrder Create(
            Guid orderId,
            Guid clientId,
            IReadOnlyList<PricedProductItem> productItems,
            decimal totalPrice
        )
        {
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.Default(clientId, nameof(clientId));
            Guard.Against.NullOrEmpty(productItems, nameof(productItems));
            Guard.Against.NegativeOrZero(totalPrice, nameof(totalPrice));

            return new InitOrder(orderId, clientId, productItems, totalPrice);
        }
    }
}

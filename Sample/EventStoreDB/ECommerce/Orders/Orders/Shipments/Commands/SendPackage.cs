using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using Core.Commands;
using Orders.Products.ValueObjects;

namespace Orders.Shipments.Commands
{
    public class SendPackage : ICommand
    {
        public Guid OrderId { get; }

        public IReadOnlyList<ProductItem> ProductItems { get; }

        private SendPackage(
            Guid orderId,
            IReadOnlyList<ProductItem> productItems
        )
        {
            OrderId = orderId;
            ProductItems = productItems;
        }

        public static SendPackage Create(
            Guid orderId,
            IReadOnlyList<ProductItem> productItems
        )
        {
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.NullOrEmpty(productItems, nameof(productItems));

            return new SendPackage(orderId, productItems);
        }
    }
}

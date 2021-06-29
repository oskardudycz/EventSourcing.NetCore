using System;
using System.Collections.Generic;
using Core.Commands;
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
            Guid? orderId,
            Guid? clientId,
            IReadOnlyList<PricedProductItem>? productItems,
            decimal? totalPrice
        )
        {
            if (!orderId.HasValue)
                throw new ArgumentNullException(nameof(orderId));
            if (!clientId.HasValue)
                throw new ArgumentNullException(nameof(clientId));
            if (productItems == null)
                throw new ArgumentNullException(nameof(productItems));
            if (!totalPrice.HasValue)
                throw new ArgumentNullException(nameof(productItems));

            return new InitOrder(orderId.Value, clientId.Value, productItems, totalPrice.Value);
        }
    }
}

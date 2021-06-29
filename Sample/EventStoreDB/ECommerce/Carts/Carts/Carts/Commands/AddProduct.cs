using System;
using Ardalis.GuardClauses;
using Carts.Carts.ValueObjects;
using Core.Commands;

namespace Carts.Carts.Commands
{
    public class AddProduct: ICommand
    {

        public Guid CartId { get; }

        public ProductItem ProductItem { get; }

        private AddProduct(Guid cartId, ProductItem productItem)
        {
            CartId = cartId;
            ProductItem = productItem;
        }
        public static AddProduct Create(Guid cartId, ProductItem productItem)
        {
            Guard.Against.Default(cartId, nameof(cartId));
            Guard.Against.Null(productItem, nameof(productItem));

            return new AddProduct(cartId, productItem);
        }
    }
}

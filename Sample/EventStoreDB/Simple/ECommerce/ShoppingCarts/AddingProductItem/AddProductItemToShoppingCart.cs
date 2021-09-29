using System;
using ECommerce.Pricing.ProductPricing;
using ECommerce.ShoppingCarts.ProductItems;

namespace ECommerce.ShoppingCarts.AddingProductItem
{
    public record AddProductItemToShoppingCart(
        Guid ShoppingCartId,
        ProductItem ProductItem,
        uint Version
    )
    {
        public static AddProductItemToShoppingCart From(Guid? cartId, ProductItem? productItem, uint? version)
        {
            if (cartId == null || cartId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cartId));
            if (productItem == null)
                throw new ArgumentOutOfRangeException(nameof(productItem));
            if (version == null)
                throw new ArgumentOutOfRangeException(nameof(version));

            return new AddProductItemToShoppingCart(cartId.Value, productItem, version.Value);
        }

        public static ProductItemAddedToShoppingCart Handle(
            IProductPriceCalculator productPriceCalculator,
            AddProductItemToShoppingCart command,
            ShoppingCart shoppingCart
        )
        {
            var (cartId, productItem, _) = command;

            if(shoppingCart.Status != ShoppingCartStatus.Pending)
                throw new InvalidOperationException($"Adding product item for cart in '{shoppingCart.Status}' status is not allowed.");

            var pricedProductItem = productPriceCalculator.Calculate(productItem);

            return new ProductItemAddedToShoppingCart(
                cartId,
                pricedProductItem
            );
        }
    }
}

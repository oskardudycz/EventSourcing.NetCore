using System;

namespace ECommerce.ShoppingCarts.ProductItems
{
    public record PricedProductItem(
        ProductItem ProductItem,
        decimal UnitPrice
    )
    {
        public Guid ProductId => ProductItem.ProductId;
        public int Quantity => ProductItem.Quantity;

        public decimal TotalPrice => Quantity * UnitPrice;

        public static PricedProductItem From(ProductItem productItem, decimal? unitPrice)
        {
            return unitPrice switch
            {
                null => throw new ArgumentNullException(nameof(unitPrice)),
                <= 0 => throw new ArgumentOutOfRangeException(nameof(unitPrice),
                    "Unit price has to be positive number"),
                _ => new PricedProductItem(productItem, unitPrice.Value)
            };
        }

        public bool MatchesProductAndUnitPrice(PricedProductItem pricedProductItem)
        {
            return ProductId == pricedProductItem.ProductId && UnitPrice == pricedProductItem.UnitPrice;
        }

        public PricedProductItem MergeWith(PricedProductItem productItem)
        {
            if(ProductId != productItem.ProductId)
                throw new ArgumentException("Product ids do not match.");
            if(UnitPrice != productItem.UnitPrice)
                throw new ArgumentException("Product unit prices do not match.");

            return new PricedProductItem(
                new ProductItem(ProductId, productItem.Quantity + productItem.Quantity),
                UnitPrice
            );
        }
    }
}

using System;

namespace MarketBasketAnalytics.Carts.ProductItems
{
    public record ProductItem(
        Guid ProductId,
        int Quantity
    );

    public record PricedProductItem(
        ProductItem ProductItem,
        decimal UnitPrice
    )
    {
        public Guid ProductId => ProductItem.ProductId;
        public int Quantity => ProductItem.Quantity;

        public decimal TotalPrice => ProductItem.Quantity * UnitPrice;

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

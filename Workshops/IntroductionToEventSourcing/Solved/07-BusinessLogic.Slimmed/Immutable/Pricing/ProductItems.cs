using System.Collections.Immutable;
using IntroductionToEventSourcing.BusinessLogic.Slimmed.Tools;

namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable.Pricing;

using ProductId = Guid;
using ProductIdWithPrice = string;
using Quantity = int;
using Price = decimal;

public record ProductItem(ProductId ProductId, Quantity Quantity);

public record PricedProductItem(
    ProductId ProductId,
    Quantity Quantity,
    Price UnitPrice
)
{
    public Price TotalPrice => Quantity * UnitPrice;
}

public class ProductItems(ImmutableDictionary<ProductIdWithPrice, Quantity> Items)
{
    public static ProductItems Empty => new ProductItems(ImmutableDictionary<ProductIdWithPrice, Quantity>.Empty);

    public ProductItems Add(PricedProductItem productItem) =>
        new(Items.Set(Key(productItem), currentQuantity => currentQuantity + productItem.Quantity));

    public ProductItems Remove(PricedProductItem productItem) =>
        new(Items.Set(Key(productItem), currentQuantity => currentQuantity - productItem.Quantity));

    public bool HasEnough(PricedProductItem productItem) =>
        Items.TryGetValue(Key(productItem), out var currentQuantity) && currentQuantity >= productItem.Quantity;

    private static ProductIdWithPrice Key(PricedProductItem pricedProductItem) =>
        $"{pricedProductItem.ProductId}_{pricedProductItem.UnitPrice}";
}

namespace Decider;

public record ProductItem(
    Guid ProductId,
    int Quantity
);

public record PricedProductItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);

public class ProductItems
{
    public static ProductItems Empty = new([]);

    public PricedProductItem[] Values { get; }

    private ProductItems(PricedProductItem[] values) => Values = values;

    public ProductItems Add(PricedProductItem productItem) =>
        new(
            Values
                .Concat([productItem])
                .GroupBy(pi => pi.ProductId)
                .Select(group => group.Count() == 1
                    ? group.First()
                    : group.First() with { Quantity = group.Sum(pi => pi.Quantity) }
                )
                .ToArray()
        );

    public ProductItems Remove(PricedProductItem productItem) =>
        new(
            Values
                .Select(pi => pi.ProductId == productItem.ProductId
                    ? pi with { Quantity = pi.Quantity - productItem.Quantity }
                    : pi
                )
                .Where(pi => pi.Quantity > 0)
                .ToArray()
        );
}

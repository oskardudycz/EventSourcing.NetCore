namespace ECommerce.ShoppingCarts.ProductItems;

public class ProductItemsList
{
    private readonly List<PricedProductItem> items;

    public ProductItemsList(List<PricedProductItem> items)
    {
        this.items = items;
    }

    public ProductItemsList Add(PricedProductItem productItem)
    {
        var clone = new List<PricedProductItem>(items);

        var currentProductItem = Find(productItem);

        if (currentProductItem == null)
            clone.Add(productItem);
        else
            clone[clone.IndexOf(currentProductItem)] = currentProductItem.MergeWith(productItem);

        return new ProductItemsList(clone);
    }

    public ProductItemsList Remove(PricedProductItem productItem)
    {
        var clone = new List<PricedProductItem>(items);

        var currentProductItem = Find(productItem);

        if (currentProductItem == null)
            throw new InvalidOperationException("Product item wasn't found");

        clone.Remove(currentProductItem);

        return new ProductItemsList(clone);
    }

    public PricedProductItem? Find(PricedProductItem productItem) =>
        items.SingleOrDefault(
            pi => pi.MatchesProductAndUnitPrice(productItem)
        );

    public static ProductItemsList Empty() =>
        new(new List<PricedProductItem>());

    public override string ToString() =>
        $"[{string.Join(", ", items.Select(pi => pi.ToString()))}]";
}
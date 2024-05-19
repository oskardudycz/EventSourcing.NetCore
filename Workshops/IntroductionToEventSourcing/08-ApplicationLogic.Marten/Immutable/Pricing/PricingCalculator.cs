namespace ApplicationLogic.Marten.Immutable.Pricing;

public interface IProductPriceCalculator
{
    PricedProductItem Calculate(ProductItem productItems);
}

public class FakeProductPriceCalculator: IProductPriceCalculator
{
    private readonly int value;

    private FakeProductPriceCalculator(int value) =>
        this.value = value;

    public static FakeProductPriceCalculator Returning(int value) => new(value);

    public PricedProductItem Calculate(ProductItem productItem)
    {
        var (productId, quantity) = productItem;
        return new PricedProductItem(productId, quantity, value);
    }
}

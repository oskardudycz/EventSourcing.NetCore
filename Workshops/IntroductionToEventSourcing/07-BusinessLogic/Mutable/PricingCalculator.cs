namespace IntroductionToEventSourcing.BusinessLogic.Mutable;

public interface IProductPriceCalculator
{
    PricedProductItem Calculate(ProductItem productItems);
}

public class FakeProductPriceCalculator: IProductPriceCalculator
{
    private readonly int value;

    private FakeProductPriceCalculator(int value)
    {
        this.value = value;
    }

    public static FakeProductPriceCalculator Returning(int value) => new(value);

    public PricedProductItem Calculate(ProductItem productItem) =>
        new() { ProductId = productItem.ProductId, Quantity = productItem.Quantity, UnitPrice = value };
}

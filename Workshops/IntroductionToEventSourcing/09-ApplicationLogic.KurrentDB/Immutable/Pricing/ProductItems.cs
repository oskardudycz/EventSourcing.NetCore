namespace ApplicationLogic.KurrentDB.Immutable.Pricing;

public record PricedProductItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
)
{
    public decimal TotalPrice => Quantity * UnitPrice;
}

public record ProductItem(Guid ProductId, int Quantity);


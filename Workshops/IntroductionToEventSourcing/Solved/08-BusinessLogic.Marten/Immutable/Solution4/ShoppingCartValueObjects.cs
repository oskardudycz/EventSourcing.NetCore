namespace IntroductionToEventSourcing.BusinessLogic.Immutable.Solution4;

public record ProductItem(
    ProductId ProductId,
    ProductQuantity Quantity
);

public record PricedProductItem(
    ProductId ProductId,
    ProductQuantity Quantity,
    ProductPrice UnitPrice
);

public record ShoppingCartId(Guid Value)
{
    public static ShoppingCartId From(Guid? value) =>
        (value != null && value != Guid.Empty)
            ? new ShoppingCartId(value.Value)
            : throw new ArgumentOutOfRangeException(nameof(value));
}

public record ClientId(Guid Value)
{
    public static ClientId From(Guid? value) =>
        (value.HasValue && value != Guid.Empty)
            ? new ClientId(value.Value)
            : throw new ArgumentOutOfRangeException(nameof(value));
}

public record ProductId(Guid Value)
{
    public static ProductId From(Guid? value) =>
        (value.HasValue && value != Guid.Empty)
            ? new ProductId(value.Value)
            : throw new ArgumentOutOfRangeException(nameof(value));
}

public record ProductQuantity(int Value):
    IComparable<ProductQuantity>,
    IComparable<int>
{
    public static ProductQuantity From(int? value) =>
        value is > 0
            ? new ProductQuantity(value.Value)
            : throw new ArgumentOutOfRangeException(nameof(value));

    public int CompareTo(ProductQuantity? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Value.CompareTo(other.Value);
    }

    public int CompareTo(int other) =>
        Value.CompareTo(other);
}

public record ProductPrice(decimal Value)
{
    public static ProductPrice From(decimal? value) =>
        value is > 0
            ? new ProductPrice(value.Value)
            : throw new ArgumentOutOfRangeException(nameof(value));
}

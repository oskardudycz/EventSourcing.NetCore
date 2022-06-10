namespace Carts.ShoppingCarts.Products;

public struct Money: IEquatable<Money>
{
    private decimal Value { get; }

    public Money(decimal value)
    {
        this.Value = value;

    }

    public bool Equals(Money other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        return obj is Money money && Equals(money);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((Value.GetHashCode()) * 397);
        }
    }

    public static bool operator ==(Money a, Money b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Money a, Money b)
    {
        return !(a == b);
    }

    public static Money operator *(Money a, int quantity)
    {
        return new Money(a.Value * quantity);
    }

    public static Money operator +(Money a, Money b)
    {
        return new Money(a.Value + b.Value);
    }
}

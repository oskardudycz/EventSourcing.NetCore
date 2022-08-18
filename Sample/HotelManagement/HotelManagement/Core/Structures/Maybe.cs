namespace HotelManagement.Core;

public class Maybe<TSomething>
{
    private readonly TSomething? value;
    public bool IsPresent { get; }

    private Maybe(TSomething value, bool isPresent)
    {
        this.value = value;
        this.IsPresent = isPresent;
    }

    public static readonly Maybe<TSomething> Empty = new(default!, false);

    public static Maybe<TSomething> Of(TSomething value) => value != null ? new Maybe<TSomething>(value, true) : Empty;

    public TSomething GetOrThrow() =>
        IsPresent ? value! : throw new ArgumentNullException(nameof(value));

    public TSomething GetOrDefault(TSomething defaultValue = default!) =>
        IsPresent ? value ?? defaultValue : defaultValue;
}

namespace Core.Structures;

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

    public static Maybe<TSomething> Of(TSomething value) =>
        value != null ? new Maybe<TSomething>(value, true) : Empty;

    public static Maybe<TSomething> If(bool check, Func<TSomething> getValue) =>
        check ? new Maybe<TSomething>(getValue(), true) : Empty;

    public TSomething GetOrThrow() =>
        IsPresent ? value! : throw new ArgumentNullException(nameof(value));

    public TSomething GetOrDefault(TSomething defaultValue = default!) =>
        IsPresent ? value ?? defaultValue : defaultValue;

    public void IfExists(Action<TSomething> perform)
    {
        if (IsPresent)
        {
            perform(value!);
        }
    }
}

public static class Maybe
{
    public static Maybe<TSomething> Of<TSomething>(TSomething value) =>
        Maybe<TSomething>.Of(value);

    public static Maybe<TSomething> If<TSomething>(bool check, Func<TSomething> getValue) =>
        Maybe<TSomething>.If(check, getValue);

}

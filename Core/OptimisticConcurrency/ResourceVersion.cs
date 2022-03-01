
namespace Core.OptimisticConcurrency;

public interface IExpectedResourceVersionProvider
{
    string? Value { get; }
    bool TrySet(string value);
}

public class DefaultExpectedResourceVersionProvider: IExpectedResourceVersionProvider
{
    public string? Value { get; private set; }

    public bool TrySet(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        Value = value;
        return true;
    }
}

public interface INextResourceVersionProvider
{
    string? Value { get; }

    bool TrySet(string value);
}

public class DefaultNextResourceVersionProvider: INextResourceVersionProvider
{
    public string? Value { get; set; }

    public bool TrySet(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        Value = value;
        return true;
    }
}

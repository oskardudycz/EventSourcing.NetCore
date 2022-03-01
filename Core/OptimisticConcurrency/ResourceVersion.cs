using System;

namespace Core.OptimisticConcurrency;

public interface IExpectedResourceVersionProvider
{
    string? Value { get; }
    bool TrySet(string value);
}

public class ExpectedResourceVersionProvider: IExpectedResourceVersionProvider
{
    private readonly Func<string, bool>? customTrySet;

    public ExpectedResourceVersionProvider(Func<string, bool>? customTrySet = null)
    {
        this.customTrySet = customTrySet;
    }

    public string? Value { get; private set; }

    public bool TrySet(string value)
    {
        if (customTrySet != null)
            return customTrySet(value);

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

public class NextResourceVersionProvider: INextResourceVersionProvider
{
    private readonly Func<string?>? customGet;
    private string? value;

    public NextResourceVersionProvider(Func<string?>? customGet = null)
    {
        this.customGet = customGet;
    }

    public string? Value
    {
        get => customGet != null ? customGet() : value;
        private set => this.value = value;
    }

    public bool TrySet(string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
            return false;

        Value = newValue;
        return true;
    }
}

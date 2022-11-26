namespace Core.Extensions;

public static class DictionaryExtensions
{

    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        Dictionary<TKey, TValue> first,
        Dictionary<TKey, TValue> second
    ) where TKey : notnull
        => new(first.Union(second));

    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        IEnumerable<KeyValuePair<TKey, TValue>> first,
        IEnumerable<KeyValuePair<TKey, TValue>> second
    ) where TKey : notnull
        => new(first.Union(second));
}

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

    public static Dictionary<TKey, TValue> With<TKey, TValue>(
        this Dictionary<TKey, TValue> first,
        TKey key,
        TValue value
    ) where TKey : notnull
    {
        var newDictionary = first.ToDictionary(ks => ks.Key, vs => vs.Value);

        newDictionary[key] = value;

        return newDictionary;
    }
}

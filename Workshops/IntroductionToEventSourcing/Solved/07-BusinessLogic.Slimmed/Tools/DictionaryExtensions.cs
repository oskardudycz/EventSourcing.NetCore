using System.Collections.Immutable;

namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Tools;


public static class DictionaryExtensions
{
    public static ImmutableDictionary<TKey, TValue> Set<TKey, TValue>(
        this ImmutableDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TValue?, TValue> set
    ) where TKey : notnull =>
        dictionary.SetItem(key, set(dictionary.TryGetValue(key, out var current) ? current : default));

    public static void Set<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TValue?, TValue> set
    ) where TKey : notnull =>
        dictionary[key] = set(dictionary.TryGetValue(key, out var current) ? current : default);
}

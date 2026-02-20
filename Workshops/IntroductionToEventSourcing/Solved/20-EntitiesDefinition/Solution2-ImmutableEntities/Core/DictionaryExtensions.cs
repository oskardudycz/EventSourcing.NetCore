namespace EntitiesDefinition.Solution2_ImmutableEntities.Core;

public static class DictionaryExtensions
{
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

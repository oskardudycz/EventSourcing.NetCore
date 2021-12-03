using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Core.Serialization;

public static class JsonObjectContractProvider
{
    private const string ConstructorAttributeName = nameof(JsonConstructorAttribute);
    private static readonly ConcurrentDictionary<Type, JsonObjectContract> Constructors = new();

    public static JsonObjectContract UsingNonDefaultConstructor(
        JsonObjectContract contract,
        Type objectType,
        Func<ConstructorInfo, JsonPropertyCollection, IList<JsonProperty>> createConstructorParameters) =>
        Constructors.GetOrAdd(objectType, (type) =>
        {
            var nonDefaultConstructor = GetNonDefaultConstructor(type);

            if (nonDefaultConstructor == null) return contract;

            contract.OverrideCreator = GetObjectConstructor(nonDefaultConstructor);
            contract.CreatorParameters.Clear();
            foreach (var constructorParameter in
                     createConstructorParameters(nonDefaultConstructor, contract.Properties))
            {
                contract.CreatorParameters.Add(constructorParameter);
            }

            return contract;
        });

    private static ObjectConstructor<object> GetObjectConstructor(MethodBase method)
    {
        var c = method as ConstructorInfo;
        if (c != null)
            return a => c.Invoke(a);

        return a => method.Invoke(null, a)!;
    }

    private static ConstructorInfo? GetNonDefaultConstructor(Type objectType)
    {
        // Use default contract for non-object types.
        if (objectType.IsPrimitive || objectType.IsEnum)
            return null;

        return GetAttributeConstructor(objectType)
               ?? GetTheMostSpecificConstructor(objectType);
    }

    private static ConstructorInfo? GetAttributeConstructor(Type objectType)
    {
        // Use default contract for non-object types.
        if (objectType.IsPrimitive || objectType.IsEnum)
            return null;

        var constructors = objectType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(c => c.GetCustomAttributes().Any(a => a.GetType().Name == ConstructorAttributeName)).ToList();

        return constructors.Count switch
        {
            1 => constructors[0],
            > 1 => throw new JsonException($"Multiple constructors with a {ConstructorAttributeName}."),
            _ => null
        };
    }

    private static ConstructorInfo? GetTheMostSpecificConstructor(Type objectType)
    {
        var constructors = objectType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .OrderBy(e => e.GetParameters().Length);

        var mostSpecific = constructors.LastOrDefault();
        return mostSpecific;
    }
}

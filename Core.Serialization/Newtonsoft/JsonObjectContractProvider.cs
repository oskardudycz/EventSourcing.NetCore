using System.Collections.Concurrent;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Core.Serialization.Newtonsoft;

public static class JsonObjectContractProvider
{
    private static readonly Type ConstructorAttributeType = typeof(JsonConstructorAttribute);
    private static readonly ConcurrentDictionary<string, JsonObjectContract> Constructors = new();

    public static JsonObjectContract UsingNonDefaultConstructor(
        JsonObjectContract contract,
        Type objectType,
        Func<ConstructorInfo, JsonPropertyCollection, IList<JsonProperty>> createConstructorParameters) =>
        Constructors.GetOrAdd(objectType.AssemblyQualifiedName!, _ =>
        {
            var nonDefaultConstructor = GetNonDefaultConstructor(objectType);

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

        if (c == null)
            return a => method.Invoke(null, a)!;

        if (!c.GetParameters().Any())
            return _ => c.Invoke([]);

        return a => c.Invoke(a);
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
            .Where(c => c.GetCustomAttributes().Any(a => a.GetType() == ConstructorAttributeType)).ToList();

        return constructors.Count switch
        {
            1 => constructors[0],
            > 1 => throw new JsonException($"Multiple constructors with a {ConstructorAttributeType.Name}."),
            _ => null
        };
    }

    private static ConstructorInfo? GetTheMostSpecificConstructor(Type objectType) =>
        objectType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .OrderByDescending(e => e.GetParameters().Length)
            .FirstOrDefault();
}

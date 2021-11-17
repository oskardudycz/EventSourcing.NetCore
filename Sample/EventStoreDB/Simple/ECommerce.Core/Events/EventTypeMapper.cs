using System;
using System.Collections.Concurrent;
using System.Linq;

namespace ECommerce.Core.Events;

public class EventTypeMapper
{
    private static readonly EventTypeMapper Instance = new();

    private readonly ConcurrentDictionary<Type, string> TypeNameMap = new();
    private readonly ConcurrentDictionary<string, Type> TypeMap = new();

    public static string ToName<TEventType>() => ToName(typeof(TEventType));

    public static string ToName(Type eventType) => Instance.TypeNameMap.GetOrAdd(eventType, (_) =>
    {
        var eventTypeName = eventType.FullName!.Replace(".", "_");

        Instance.TypeMap.AddOrUpdate(eventTypeName, eventType, (_, _) => eventType);

        return eventTypeName;
    });

    public static Type ToType(string eventTypeName) => Instance.TypeMap.GetOrAdd(eventTypeName, (_) =>
    {
        var type = GetFirstMatchingTypeFromCurrentDomainAssembly(eventTypeName.Replace("_", "."))!;

        if (type == null)
            throw new Exception($"Type for '{eventTypeName}' wasn't found!");

        Instance.TypeNameMap.AddOrUpdate(type, eventTypeName, (_, _) => eventTypeName);

        return type;
    });

    private static Type? GetFirstMatchingTypeFromCurrentDomainAssembly(string typeName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes().Where(x => x.FullName == typeName || x.Name == typeName))
            .FirstOrDefault();
    }
}
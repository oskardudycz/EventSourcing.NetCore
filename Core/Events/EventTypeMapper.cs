using System;
using System.Collections.Concurrent;
using Core.Reflection;

namespace Core.Events;

public class EventTypeMapper
{
    private static readonly EventTypeMapper Instance = new();

    private readonly ConcurrentDictionary<string, Type?> typeMap = new();
    private readonly ConcurrentDictionary<Type, string> typeNameMap = new();

    public static void AddCustomMap<T>(string mappedEventTypeName) => AddCustomMap(typeof(T), mappedEventTypeName);

    public static void AddCustomMap(Type eventType, string mappedEventTypeName)
    {
        Instance.typeNameMap.AddOrUpdate(eventType, mappedEventTypeName, (_, _) => mappedEventTypeName);
        Instance.typeMap.AddOrUpdate(mappedEventTypeName, eventType, (_, _) => eventType);
    }

    public static string ToName<TEventType>() => ToName(typeof(TEventType));

    public static string ToName(Type eventType) => Instance.typeNameMap.GetOrAdd(eventType, _ =>
    {
        var eventTypeName = eventType.FullName!.Replace(".", "_");

        Instance.typeMap.AddOrUpdate(eventTypeName, eventType, (_, _) => eventType);

        return eventTypeName;
    });

    public static Type? ToType(string eventTypeName) => Instance.typeMap.GetOrAdd(eventTypeName, _ =>
    {
        var type = TypeProvider.GetFirstMatchingTypeFromCurrentDomainAssembly(eventTypeName.Replace("_", "."));

        if (type == null)
            return null;

        Instance.typeNameMap.AddOrUpdate(type, eventTypeName, (_, _) => eventTypeName);

        return type;
    });
}

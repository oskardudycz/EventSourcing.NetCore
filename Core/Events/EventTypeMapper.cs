using System;
using System.Collections.Concurrent;
using Core.Reflection;

namespace Core.Events;

public class EventTypeMapper
{
    private static readonly EventTypeMapper Instance = new();

    private readonly ConcurrentDictionary<Type, string> TypeNameMap = new();
    private readonly ConcurrentDictionary<string, Type> TypeMap = new();

    public static void AddCustomMap<T>(string mappedEventTypeName) => AddCustomMap(typeof(T), mappedEventTypeName);

    public static void AddCustomMap(Type eventType, string mappedEventTypeName)
    {
        Instance.TypeNameMap.AddOrUpdate(eventType, mappedEventTypeName, (_, _) => mappedEventTypeName);
        Instance.TypeMap.AddOrUpdate(mappedEventTypeName, eventType, (_, _) => eventType);
    }

    public static string ToName<TEventType>() => ToName(typeof(TEventType));

    public static string ToName(Type eventType) => Instance.TypeNameMap.GetOrAdd(eventType, (_) =>
    {
        var eventTypeName = eventType.FullName!.Replace(".", "_");

        Instance.TypeMap.AddOrUpdate(eventTypeName, eventType, (_, _) => eventType);

        return eventTypeName;
    });

    public static Type ToType(string eventTypeName) => Instance.TypeMap.GetOrAdd(eventTypeName, (_) =>
    {
        var type = TypeProvider.GetFirstMatchingTypeFromCurrentDomainAssembly(eventTypeName.Replace("_", "."))!;

        if (type == null)
            throw new Exception($"Type map for '{eventTypeName}' wasn't found!");

        Instance.TypeNameMap.AddOrUpdate(type, eventTypeName, (_, _) => eventTypeName);

        return type;
    });
}
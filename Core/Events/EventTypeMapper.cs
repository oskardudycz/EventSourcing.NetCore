using System.Collections.Concurrent;
using Core.Reflection;

namespace Core.Events;

public class EventTypeMapper
{
    public static readonly EventTypeMapper Instance = new();

    private readonly ConcurrentDictionary<string, Type?> typeMap = new();
    private readonly ConcurrentDictionary<Type, string> typeNameMap = new();

    public void AddCustomMap<T>(string eventTypeName) => AddCustomMap(typeof(T), eventTypeName);

    public void AddCustomMap(Type eventType, string eventTypeName)
    {
        typeNameMap.AddOrUpdate(eventType, eventTypeName, (_, typeName) => typeName);
        typeMap.AddOrUpdate(eventTypeName, eventType, (_, type) => type);
    }

    public string ToName<TEventType>() => ToName(typeof(TEventType));

    public string ToName(Type eventType) => typeNameMap.GetOrAdd(eventType, _ =>
    {
        var eventTypeName = eventType.FullName!;

        typeMap.TryAdd(eventTypeName, eventType);

        return eventTypeName;
    });

    public Type? ToType(string eventTypeName) => typeMap.GetOrAdd(eventTypeName, _ =>
    {
        var type = TypeProvider.GetFirstMatchingTypeFromCurrentDomainAssembly(eventTypeName);

        if (type == null)
            return null;

        typeNameMap.TryAdd(type, eventTypeName);

        return type;
    });
}

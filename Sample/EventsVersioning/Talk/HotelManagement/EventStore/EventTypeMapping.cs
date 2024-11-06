using System.Collections.Concurrent;

namespace HotelManagement.EventStore;

public class EventTypeMapping
{
    private readonly ConcurrentDictionary<string, Type?> typeMap = new();
    private readonly ConcurrentDictionary<Type, string> typeNameMap = new();

    public EventTypeMapping CustomMap<T>(params string[] eventTypeNames) => CustomMap(typeof(T), eventTypeNames);

    public EventTypeMapping CustomMap(Type eventType, params string[] eventTypeNames)
    {
        foreach (var eventTypeName in eventTypeNames)
        {
            typeNameMap.AddOrUpdate(eventType, eventTypeName, (_, typeName) => typeName);
            typeMap.AddOrUpdate(eventTypeName, eventType, (_, type) => type);
        }

        return this;
    }

    public string ToName<TEventType>() => ToName(typeof(TEventType));

    public string ToName(Type eventType) =>
        typeNameMap.GetOrAdd(eventType, _ =>
        {
            var eventTypeName = eventType.FullName!;

            typeMap.TryAdd(eventTypeName, eventType);

            return eventTypeName;
        });

    public Type? ToType(string eventTypeName) =>
        typeMap.GetOrAdd(eventTypeName, _ =>
        {
            var type = GetFirstMatchingTypeFromCurrentDomainAssembly(eventTypeName);

            if (type == null)
                return null;

            typeNameMap.TryAdd(type, eventTypeName);

            return type;
        });

    private static Type? GetFirstMatchingTypeFromCurrentDomainAssembly(string typeName) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes().Where(x => x.FullName == typeName || x.Name == typeName))
            .FirstOrDefault();
}

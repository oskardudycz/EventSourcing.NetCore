using Marten;

namespace ApplicationLogic.Marten.Core.Marten;

public static class EventMappings
{
    public static StoreOptions MapEventWithPrefix<T>(this StoreOptions options, string prefix) where T : class
    {
        options.Events.MapEventType<T>($"{prefix}-${typeof(T).FullName}");
        return options;
    }
}

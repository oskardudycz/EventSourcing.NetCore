using System.Text.Json;

namespace IntroductionToEventSourcing.BusinessLogic.Tools;

public class EventStore
{
    private readonly Dictionary<Guid, List<(string EventType, string Json)>> events = new();

    public void AppendToStream(Guid streamId, IEnumerable<object> newEvents)
    {
        if (!events.ContainsKey(streamId))
            events[streamId] = [];

        events[streamId].AddRange(newEvents.Select(e => (e.GetType().FullName!, JsonSerializer.Serialize(e))));
    }

    public TEvent[] ReadStream<TEvent>(Guid streamId) where TEvent : notnull =>
        events.TryGetValue(streamId, out var stream)
            ? stream.Select(@event =>
                    JsonSerializer.Deserialize(@event.Json, Type.GetType(@event.EventType)!)
                )
                .Where(e => e != null).Cast<TEvent>().ToArray()
            : [];
}
